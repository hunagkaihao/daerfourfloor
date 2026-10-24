using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Erp;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.StockOutHistories.Aggregates;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using TuTa.Wms.Stocks.ValueObjects;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace TuTa.Wms.Stocks
{
    public partial class StockService
    {
        /// <summary>柜门可扫描纯存货编码或逗号卡码，第1段物料、第2段可选批号、第4段可选等级。</summary>
        private static (string Material, string Batch, string Grade) ParseRemainderMaterialScan(string scanCode)
        {
            var fields = (scanCode ?? "").Trim().Split(',').Select(x => x.Trim()).ToArray();
            if (string.IsNullOrWhiteSpace(fields[0]) || (fields.Length != 1 && fields.Length < 4))
                throw new UserFriendlyException("请扫描存货编码或完整物料等级卡，例如622550001,,,Z1,,,");
            return (fields[0], fields.Length > 1 ? fields[1] : "", fields.Length > 3 ? fields[3] : "");
        }

        /// <summary>仅加载标记为零头柜区的手工格位，查询不包含4楼叉车库的其他库存。</summary>
        private async Task<List<Cell>> GetRemainderCellsAsync()
        {
            var warehouses = await _warehouseRepository.GetAllWarehousesAsync();
            var areas = warehouses.SelectMany(x => x.WarehouseAreas)
                .Where(x => x.WarehouseAreaFlag == RemainderAreaFlag).ToList();
            var areaIds = areas.Select(x => x.Id).ToList();
            if (areaIds.Count == 0) return new List<Cell>();
            var cells = await _cellRepository.GetListAsync(x => x.CellType == CellType.HandCell &&
                x.WarehouseAreaId.HasValue && areaIds.Contains(x.WarehouseAreaId.Value));
            return cells.Where(x => areas.Any(a => a.Id == x.WarehouseAreaId && a.WarehouseId == x.WarehouseId) &&
                Regex.IsMatch(x.CellCode, @"^[A-L](0[1-9]|1[0-5])-0[1-8]$")).ToList();
        }

        /// <summary>独立寄存直接可用；历史报检入库仍保留原检验约束，冻结及占用库存不能领用。</summary>
        private static bool CanWithdrawRemainderStock(Stock stock)
            => stock.TotalCountInTime > 0 && stock.Status == StockStatus.Available &&
               (stock.StockInType == StockInType.RemainderDeposit || stock.InspectionStatus == InspectionStatus.InspectionQualified) &&
               string.IsNullOrWhiteSpace(stock.U8OutboundRecordId) && !stock.BoxData.BoxId.HasValue &&
               (stock.InspectionCount ?? 0) == 0;

        /// <summary>寄存与领用均要求有效的存货、等级、柜位及正数量，按既有decimal(18,6)精度校验。</summary>
        private static bool IsValidRemainderOperation(RemainderCabinetDepositDto input)
            => input != null && input.RequestId != Guid.Empty && !string.IsNullOrWhiteSpace(input.MaterialCode) &&
               input.MaterialCode.Trim().Length <= 20 && !string.IsNullOrWhiteSpace(input.Grade) &&
               input.Grade.Trim().Length <= 20 && !string.IsNullOrWhiteSpace(input.CellCode) &&
               input.Quantity > 0 && input.Quantity <= 999999999999.999999m && input.Quantity == decimal.Round(input.Quantity, 6);

        /// <summary>规范同一次操作的内容摘要，用于事务内登记及网络重试核实。</summary>
        private static string GetRemainderOperationFingerprint(RemainderCabinetDepositDto input, string cellCode)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {
                CellCode = cellCode, MaterialCode = input.MaterialCode.Trim(), Grade = input.Grade.Trim(), input.Quantity
            }))));

        /// <summary>独立寄存仅写入统一库存、固定格位和入库流水，不查询或改写ASN，不调用U8。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<ResponseDto> DepositRemainderAsync(RemainderCabinetDepositDto input)
        {
            if (!IsValidRemainderOperation(input))
                return new ResponseDto { success = false, message = "请提供存货编码、等级、柜位及有效的正数量，数量最多6位小数" };
            var written = false;
            await ErpAsnAppService.LocalPushLock.WaitAsync();
            try
            {
                var cellCode = NormalizeRemainderCellCode(input.CellCode);
                await using var cellHandle = _u8GroupingLock == null ? null : await _u8GroupingLock.TryAcquireAsync("U8:REMAINDER:" + cellCode);
                if (_u8GroupingLock != null && cellHandle == null)
                    return new ResponseDto { success = false, message = "该柜位正在寄存或领用，请稍后重试" };
                using (var uow = UnitOfWorkManager.Begin(true, true))
                {
                    var location = await ResolveRemainderCellAsync(cellCode);
                    var fingerprint = GetRemainderOperationFingerprint(input, cellCode);
                    var previous = location.Cell.GetRemainderDepositFingerprint(input.RequestId);
                    if (previous != null)
                        return new ResponseDto { success = previous == fingerprint, message = previous == fingerprint
                            ? "该次寄存已完成，无需重复增加库存" : "该提交标识已用于其他寄存数据，请核对原请求" };
                    if (location.Cell.RunStatus != CellRunStatus.Enable || location.Cell.CellStatus == CellStatus.Full)
                        return new ResponseDto { success = false, message = "该零头柜位已禁用或标记满货" };
                    if (await _stockRepository.FindAsync(input.RequestId) != null)
                        return new ResponseDto { success = false, message = "该提交标识已有关联库存，请核对原记录" };
                    var materialCode = input.MaterialCode.Trim(); var grade = input.Grade.Trim();
                    var material = await _materialRepository.FindByMaterialCodeAsync(materialCode);
                    if (material == null) return new ResponseDto { success = false, message = "存货主数据不存在，请先同步物料" };
                    var existing = await _stockRepository.GetByCellIdAsync(location.Cell.Id);
                    if (existing.Any(x => (x.TotalCountInTime > 0 || (x.InspectionCount ?? 0) > 0) &&
                        (x.Material.MaterialCode != materialCode || x.Grade != grade)))
                        return new ResponseDto { success = false, message = "该柜位已有其他存货或等级，不允许混放" };
                    location.Cell.BindRemainderMaterial(materialCode, grade);
                    var stock = _stocksManager.CreateRemainderDepositStock(input.RequestId, input.Quantity,
                        new MaterialInfoOfStock(material.MaterialCode, material.MaterialName, material.Specs, material.Unit, material.FinGoodsList));
                    stock.BindCell(location.Cell, location.Warehouse, location.Area);
                    stock.SetGrade(grade);
                    stock.StockInDate = DateTime.Now;
                    var history = new StockInHistory(materialCode, materialCode, material.MaterialName, material.Specs,
                        material.Unit, location.Warehouse.WarehouseCode, location.Warehouse.WarehouseName,
                        location.Area.WarehouseAreaCode, location.Area.WarehouseAreaName, cellCode, location.Cell.CellName,
                        null, null, "零头柜寄存", input.Quantity, DateTime.Now, CurrentUser.UserName);
                    history.SetProperty("Grade", grade);
                    history.SetProperty("RemainderDepositId", input.RequestId.ToString("N"));
                    location.Cell.RecordRemainderDeposit(input.RequestId, fingerprint);
                    location.Cell.SetCellStatus(CellStatus.Have);
                    written = true;
                    await _stockRepository.InsertAsync(stock, autoSave: true);
                    await _stockInHistoryRepository.InsertAsync(history);
                    await _cellRepository.UpdateAsync(location.Cell);
                    await uow.CompleteAsync();
                    return new ResponseDto { success = true, message = $"已向{cellCode}寄存{materialCode}/{grade}，数量{input.Quantity}{material.Unit}" };
                }
            }
            catch (UserFriendlyException ex) when (!written) { return new ResponseDto { success = false, message = ex.Message }; }
            finally { ErpAsnAppService.LocalPushLock.Release(); }
        }

        /// <summary>查询仅限零头柜的统一库存；扫码存货或柜门，文本条件按包含匹配。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<PagedResultDto<RemainderCabinetBalanceDto>> GetRemainderStocksAsync(RemainderCabinetQueryDto input)
        {
            input ??= new RemainderCabinetQueryDto();
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                var cells = await GetRemainderCellsAsync();
                var cellFilter = input.CellCode?.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(cellFilter)) cells = cells.Where(x => x.CellCode.Contains(cellFilter)).ToList();
                (string Material, string Batch, string Grade)? scan = null;
                if (!string.IsNullOrWhiteSpace(input.ScanCode))
                {
                    if (Regex.IsMatch(input.ScanCode.Trim().ToUpperInvariant(), @"^[A-L][0-9]{1,2}-[0-9]{1,2}$"))
                    {
                        var exactCell = NormalizeRemainderCellCode(input.ScanCode);
                        cells = cells.Where(x => x.CellCode == exactCell).ToList();
                    }
                    else scan = ParseRemainderMaterialScan(input.ScanCode);
                }
                var ids = cells.Select(x => x.Id).ToList();
                if (ids.Count == 0)
                    return new PagedResultDto<RemainderCabinetBalanceDto>(0, new List<RemainderCabinetBalanceDto>());
                var query = (await _stockRepository.GetQueryableAsync()).Where(x => x.TotalCountInTime > 0 &&
                    x.CellData.CellId.HasValue && ids.Contains(x.CellData.CellId.Value));
                if (scan.HasValue)
                {
                    var material = scan.Value.Material; var grade = scan.Value.Grade;
                    query = query.Where(x => x.Material.MaterialCode == material);
                    if (!string.IsNullOrEmpty(grade)) query = query.Where(x => x.Grade == grade);
                }
                if (!string.IsNullOrWhiteSpace(input.MaterialCode))
                {
                    var material = input.MaterialCode.Trim();
                    query = query.Where(x => x.Material.MaterialCode.Contains(material));
                }
                if (!string.IsNullOrWhiteSpace(input.Grade))
                {
                    var grade = input.Grade.Trim();
                    query = query.Where(x => x.Grade != null && x.Grade.Contains(grade));
                }
                var balances = query.GroupBy(x => new { x.CellData.CellCode, x.Material.MaterialCode, x.Material.MaterialName, x.Grade, x.Material.Unit })
                    .Select(g => new RemainderCabinetBalanceDto {
                        CellCode = g.Key.CellCode, MaterialCode = g.Key.MaterialCode, MaterialName = g.Key.MaterialName,
                        Grade = g.Key.Grade, Unit = g.Key.Unit, Quantity = g.Sum(x => x.TotalCountInTime),
                        AvailableQuantity = g.Sum(x => x.Status == StockStatus.Available &&
                            (x.StockInType == StockInType.RemainderDeposit || x.InspectionStatus == InspectionStatus.InspectionQualified) &&
                            (x.U8OutboundRecordId == null || x.U8OutboundRecordId == "") && x.BoxData.BoxId == null &&
                            (x.InspectionCount ?? 0) == 0 ? x.TotalCountInTime : 0)
                    });
                var count = await AsyncExecuter.CountAsync(balances);
                var items = await AsyncExecuter.ToListAsync(balances.OrderBy(x => x.CellCode).ThenBy(x => x.MaterialCode).ThenBy(x => x.Grade)
                    .Skip(Math.Max(input.SkipCount, 0)).Take(Math.Clamp(input.MaxResultCount, 1, 200)));
                var enabledCodes = cells.Where(x => x.RunStatus == CellRunStatus.Enable).Select(x => x.CellCode).ToHashSet();
                foreach (var item in items)
                    if (!enabledCodes.Contains(item.CellCode)) item.AvailableQuantity = 0;
                await read.CompleteAsync();
                return new PagedResultDto<RemainderCabinetBalanceDto>(count, items);
            }
        }

        /// <summary>将年、月、日查询转换为闭开时间范围，保证全天记录均能查出。</summary>
        private static (DateTime? Start, DateTime? End) GetRemainderHistoryDates(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return (null, null);
            keyword = keyword.Trim();
            var format = keyword.Length == 4 ? "yyyy" : keyword.Length == 7 ? "yyyy-MM" : "yyyy-MM-dd";
            if (!DateTime.TryParseExact(keyword, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
                start.Year >= 9999)
                throw new UserFriendlyException("日期请输入有效的yyyy、yyyy-MM或yyyy-MM-dd，例如2026-10");
            start = new DateTime(start.Year, keyword.Length == 4 ? 1 : start.Month, keyword.Length <= 7 ? 1 : start.Day);
            return (start, keyword.Length == 4 ? start.AddYears(1) : keyword.Length == 7 ? start.AddMonths(1) : start.AddDays(1));
        }

        /// <summary>在数据库内合并并分页寄存、领用流水，固定格位等级兼容此前未记录Grade扩展项的流水。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<PagedResultDto<RemainderCabinetHistoryDto>> GetRemainderHistoriesAsync(RemainderCabinetQueryDto input)
        {
            input ??= new RemainderCabinetQueryDto();
            var dates = GetRemainderHistoryDates(input.DateKeyword);
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                var cells = await GetRemainderCellsAsync();
                if (!string.IsNullOrWhiteSpace(input.CellCode))
                    cells = cells.Where(x => x.CellCode.Contains(input.CellCode.Trim().ToUpperInvariant())).ToList();
                if (!string.IsNullOrWhiteSpace(input.Grade))
                    cells = cells.Where(x => (x.RemainderGrade ?? "").Contains(input.Grade.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                var codes = cells.Select(x => x.CellCode).ToList();
                if (codes.Count == 0)
                    return new PagedResultDto<RemainderCabinetHistoryDto>(0, new List<RemainderCabinetHistoryDto>());
                var incoming = (await _stockInHistoryRepository.GetQueryableAsync())
                    .Where(x => (x.StockInType == "零头柜寄存" || x.StockInType == "ASN零头柜入库") && codes.Contains(x.CellCode))
                    .Select(x => new RemainderCabinetHistoryDto {
                        HistoryId = x.Id, Operation = "寄存", OperationTime = x.InTime, MaterialCode = x.MaterialCode,
                        MaterialName = x.MaterialName, CellCode = x.CellCode, BatchCode = x.BatchNo,
                        Quantity = x.InCount, Unit = x.MaterialUnit, OperatorName = x.OperatorName
                    });
                var outgoing = (await _stockOutHistoryRepository.GetQueryableAsync())
                    .Where(x => x.StockOutType == "零头柜领用" && codes.Contains(x.CellCode))
                    .Select(x => new RemainderCabinetHistoryDto {
                        HistoryId = x.Id, Operation = "领用", OperationTime = x.OutTime, MaterialCode = x.MaterialCode,
                        MaterialName = x.MaterialName, CellCode = x.CellCode, BatchCode = x.BatchNo,
                        Quantity = x.OutCount, Unit = x.MaterialUnit, OperatorName = x.OperatorName
                    });
                var query = incoming.Concat(outgoing);
                if (!string.IsNullOrWhiteSpace(input.MaterialCode))
                {
                    var material = input.MaterialCode.Trim();
                    query = query.Where(x => x.MaterialCode.Contains(material));
                }
                if (dates.Start.HasValue) query = query.Where(x => x.OperationTime >= dates.Start.Value);
                if (dates.End.HasValue) query = query.Where(x => x.OperationTime < dates.End.Value);
                var count = await AsyncExecuter.CountAsync(query);
                var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.OperationTime)
                    .ThenByDescending(x => x.HistoryId).ThenBy(x => x.Operation)
                    .Skip(Math.Max(input.SkipCount, 0)).Take(Math.Clamp(input.MaxResultCount, 1, 200)));
                var grades = cells.ToDictionary(x => x.CellCode, x => x.RemainderGrade);
                foreach (var row in rows) row.Grade = grades[row.CellCode];
                await read.CompleteAsync();
                return new PagedResultDto<RemainderCabinetHistoryDto>(count, rows);
            }
        }

        /// <summary>按存货、等级及柜位领用，按入库顺序扣减可用数量，不关联或改写ASN，不调用U8。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<ResponseDto> WithdrawRemainderAsync(RemainderCabinetWithdrawDto input)
        {
            if (!IsValidRemainderOperation(input))
                return new ResponseDto { success = false, message = "请提供存货编码、等级、柜位及有效的正数量，数量最多6位小数" };
            var written = false;
            await ErpAsnAppService.LocalPushLock.WaitAsync();
            try
            {
                var cellCode = NormalizeRemainderCellCode(input.CellCode);
                await using var cellHandle = _u8GroupingLock == null ? null : await _u8GroupingLock.TryAcquireAsync("U8:REMAINDER:" + cellCode);
                if (_u8GroupingLock != null && cellHandle == null)
                    return new ResponseDto { success = false, message = "该柜位正在寄存或领用，请稍后重试" };
                using (var uow = UnitOfWorkManager.Begin(true, true))
                {
                    var location = await ResolveRemainderCellAsync(cellCode);
                    var fingerprint = GetRemainderOperationFingerprint(input, cellCode);
                    var previous = location.Cell.GetRemainderWithdrawalFingerprint(input.RequestId);
                    if (previous != null)
                        return new ResponseDto { success = previous == fingerprint, message = previous == fingerprint
                            ? "该次领用已完成，无需重复扣减库存" : "该提交标识已用于其他领用数据，请核对原请求" };
                    if (location.Cell.RunStatus != CellRunStatus.Enable)
                        return new ResponseDto { success = false, message = "该柜位已禁用，不能领用" };
                    var materialCode = input.MaterialCode.Trim(); var grade = input.Grade.Trim();
                    if (materialCode != location.Cell.RemainderMaterialCode || grade != location.Cell.RemainderGrade)
                        return new ResponseDto { success = false, message = "所选存货或等级与柜位固定信息不一致" };
                    var stocksInCell = await _stockRepository.GetByCellIdAsync(location.Cell.Id);
                    var available = stocksInCell.Where(x => x.Material.MaterialCode == materialCode && x.Grade == grade && CanWithdrawRemainderStock(x))
                        .OrderBy(x => x.StockInDate).ThenBy(x => x.CreationTime).ThenBy(x => x.Id).ToList();
                    if (available.Sum(x => x.TotalCountInTime) < input.Quantity)
                        return new ResponseDto { success = false, message = "领用数量超过当前可用数量，请重新查询；冻结、锁定及占用数量不能领用" };
                    var first = available[0];
                    var history = new StockOutHistory(materialCode, materialCode, first.Material.MaterialName,
                        first.Material.Specs, first.Material.Unit, location.Warehouse.WarehouseCode, location.Warehouse.WarehouseName,
                        location.Area.WarehouseAreaCode, location.Area.WarehouseAreaName, cellCode, location.Cell.CellName,
                        null, null, "零头柜领用", input.Quantity, DateTime.Now, CurrentUser.UserName);
                    history.SetProperty("RemainderWithdrawalId", input.RequestId.ToString("N"));
                    history.SetProperty("Grade", grade);
                    var remaining = input.Quantity;
                    var allocations = new List<object>();
                    written = true;
                    foreach (var stock in available)
                    {
                        if (remaining <= 0) break;
                        var taken = Math.Min(remaining, stock.TotalCountInTime);
                        stock.Remove(taken);
                        allocations.Add(new { StockId = stock.Id, Quantity = taken });
                        remaining -= taken;
                        if (stock.TotalCountInTime == 0) await _stockRepository.DeleteAsync(stock);
                        else await _stockRepository.UpdateAsync(stock);
                    }
                    history.SetProperty("Allocations", JsonConvert.SerializeObject(allocations));
                    location.Cell.RecordRemainderWithdrawal(input.RequestId, fingerprint);
                    location.Cell.SetCellStatus(stocksInCell.Any(x => x.TotalCountInTime > 0 || (x.InspectionCount ?? 0) > 0)
                        ? CellStatus.Have : CellStatus.Nohave);
                    await _stockOutHistoryRepository.InsertAsync(history);
                    await _cellRepository.UpdateAsync(location.Cell);
                    await uow.CompleteAsync();
                    return new ResponseDto { success = true, message = $"已从{cellCode}领用{materialCode}/{grade}，数量{input.Quantity}{first.Material.Unit}" };
                }
            }
            catch (UserFriendlyException ex) when (!written)
            {
                return new ResponseDto { success = false, message = ex.Message };
            }
            finally { ErpAsnAppService.LocalPushLock.Release(); }
        }
    }
}
