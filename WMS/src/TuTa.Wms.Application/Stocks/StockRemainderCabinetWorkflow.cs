using System;
using System.Collections.Generic;
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
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace TuTa.Wms.Stocks
{
    public partial class StockService
    {
        /// <summary>零头柜区标记，防止将普通叉车库位误作为零头柜。</summary>
        private const string RemainderAreaFlag = "RemainderCabinet";

        /// <summary>规范柜位格式，列从左到右01—15，层从下到上01—08。</summary>
        private static string NormalizeRemainderCellCode(string cellCode)
        {
            var match = Regex.Match((cellCode ?? "").Trim().ToUpperInvariant(), @"^([A-L])([0-9]{1,2})-([0-9]{1,2})$");
            if (!match.Success || !int.TryParse(match.Groups[2].Value, out var column) ||
                !int.TryParse(match.Groups[3].Value, out var layer) || column < 1 || column > 15 || layer < 1 || layer > 8)
                throw new UserFriendlyException("零头柜位格式应为A01-01至L15-08，例如A15-08");
            return $"{match.Groups[1].Value}{column:00}-{layer:00}";
        }

        /// <summary>读取已初始化的零头格位及所属仓库、库区，不自动占用普通库位或修改仓库归属。</summary>
        private async Task<(Cell Cell, Warehouse Warehouse, WarehouseArea Area)> ResolveRemainderCellAsync(string cellCode)
        {
            var cells = await _cellRepository.GetListAsync(x => x.CellCode == cellCode);
            if (cells.Count != 1) throw new UserFriendlyException("零头柜位不存在或编码不唯一，请先核对零头柜基础数据");
            var cell = cells[0];
            var warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
            var area = cell.WarehouseAreaId.HasValue ? warehouse?.GetAreaByAreaId(cell.WarehouseAreaId.Value) : null;
            // 所属仓库由柜位及库区的真实外键确定，初始化数据将零头柜区建在4楼叉车库下。
            if (warehouse == null || area?.WarehouseAreaFlag != RemainderAreaFlag || area.WarehouseId != warehouse.Id ||
                cell.CellType != CellType.HandCell)
                throw new UserFriendlyException("该柜位未配置为4楼叉车库零头柜区的手工库位");
            return (cell, warehouse, area);
        }

        /// <summary>查询格位固定物料及Stocks统一库存，批次和ASN来源分别展示。</summary>
        /// <param name="cellCode">A—L柜位编码，例如A15-08。</param>
        /// <returns>格位绑定及仍有数量的库存记录。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<RemainderCabinetDto> GetRemainderCabinetAsync(string cellCode)
        {
            cellCode = NormalizeRemainderCellCode(cellCode);
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                var location = await ResolveRemainderCellAsync(cellCode);
                var stocks = (await _stockRepository.GetByCellIdAsync(location.Cell.Id))
                    .Where(x => x.TotalCountInTime > 0).OrderBy(x => x.CreationTime).ToList();
                var ids = stocks.Where(x => x.U8AsnLineId.HasValue).Select(x => x.U8AsnLineId.Value).Distinct().ToList();
                var sources = ids.Count == 0 ? new Dictionary<Guid, ErpAsn>() :
                    (await _erpAsnRepository.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);
                var result = new RemainderCabinetDto {
                    CellCode = cellCode, WarehouseName = location.Warehouse.WarehouseName,
                    CanStore = location.Cell.RunStatus == CellRunStatus.Enable && location.Cell.CellStatus != CellStatus.Full,
                    MaterialCode = location.Cell.RemainderMaterialCode, Grade = location.Cell.RemainderGrade
                };
                foreach (var stock in stocks)
                {
                    var source = stock.U8AsnLineId.HasValue && sources.TryGetValue(stock.U8AsnLineId.Value, out var row) ? row : null;
                    result.Stocks.Add(new RemainderCabinetStockDto {
                        StockId = stock.Id, AsnCode = source?.AsnCode, OrderCode = source?.OrderCode,
                        MaterialCode = stock.Material.MaterialCode, MaterialName = stock.Material.MaterialName,
                        BatchCode = stock.BatchCode, Grade = stock.Grade, Quantity = stock.TotalCountInTime,
                        Status = stock.Status.ToString(), CellCode = cellCode, Unit = stock.Material.Unit,
                        CanWithdraw = CanWithdrawRemainderStock(stock) && location.Cell.RunStatus == CellRunStatus.Enable
                    });
                }
                await read.CompleteAsync();
                return result;
            }
        }

        /// <summary>ASN合格零头直接计入4楼叉车库手工库位，不生成托盘、满箱数量或AGV任务。</summary>
        /// <param name="input">稳定提交标识、明确ASN来源、柜位、标识卡及实际入柜数量。</param>
        /// <returns>库存登记结果；采购入库失败只附加待处理说明，不重复登记库存。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<ResponseDto> StoreAsnRemainderAsync(RemainderCabinetEntryDto input)
        {
            if (input == null || input.RequestId == Guid.Empty || input.AsnLineId == Guid.Empty ||
                string.IsNullOrWhiteSpace(input.AsnCode) || string.IsNullOrWhiteSpace(input.CardBarcode) || input.Quantity <= 0)
                return new ResponseDto { success = false, message = "请提供有效的提交标识、ASN明细、标识卡和实际零头数量" };
            if (input.Quantity != decimal.Round(input.Quantity, 6))
                return new ResponseDto { success = false, message = "零头数量最多支持6位小数，单位同ASN物料" };
            var written = false;
            ResponseDto result;
            await ErpAsnAppService.LocalPushLock.WaitAsync();
            try
            {
                var cellCode = NormalizeRemainderCellCode(input.CellCode);
                var scan = ParseRemainderMaterialScan(input.CardBarcode);
                var asnCode = input.AsnCode.Trim();
                await using var asnHandle = _u8GroupingLock == null ? null : await _u8GroupingLock.TryAcquireAsync("U8:ASN:" + asnCode);
                if (_u8GroupingLock != null && asnHandle == null)
                    return new ResponseDto { success = false, message = "该ASN正在处理，请稍后重试" };
                await using var cellHandle = _u8GroupingLock == null ? null : await _u8GroupingLock.TryAcquireAsync("U8:REMAINDER:" + cellCode);
                if (_u8GroupingLock != null && cellHandle == null)
                    return new ResponseDto { success = false, message = "该零头格位正在入库，请稍后重试" };
                using (var uow = UnitOfWorkManager.Begin(true, true))
                {
                    var line = await _erpAsnRepository.FindAsync(input.AsnLineId);
                    if (line == null || line.AsnCode != asnCode)
                        return new ResponseDto { success = false, message = "所选ASN明细不属于当前送货单" };
                    var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {
                        AsnCode = asnCode, input.AsnLineId, CellCode = cellCode, CardBarcode = input.CardBarcode.Trim(), input.Quantity
                    }))));
                    var previous = line.GetRemainderEntryFingerprint(input.RequestId);
                    if (previous != null)
                        return new ResponseDto { success = previous == fingerprint, message = previous == fingerprint
                            ? "该次零头入柜已登记，无需重复入账；U8采购入库可在操作区独立重试"
                            : "该提交标识已有其他入柜数据，请核对原记录，不能更换柜位、来源或数量后重复提交" };
                    if (await _stockRepository.FindAsync(input.RequestId) != null)
                        return new ResponseDto { success = false, message = "该提交标识已有关联库存，请核对原入库记录" };
                    if (line.Status == AsnStatus.Cancelled || !line.IsPushedToErp || !line.IsInspectionReportPushed ||
                        line.MaterialInspectionStatus != InspectionStatus.InspectionQualified || line.IsPurchasePushed)
                        return new ResponseDto { success = false, message = "仅已成功报检、人工确认合格且尚未采购入库的ASN剩余数量允许入柜" };
                    var grade = (line.Free5 ?? "").Trim();
                    if (scan.Material != line.MaterialCode || string.IsNullOrEmpty(grade) ||
                        (!string.IsNullOrEmpty(scan.Grade) && scan.Grade != grade) ||
                        (!string.IsNullOrEmpty(scan.Batch) && scan.Batch != line.BatchCode))
                        return new ResponseDto { success = false, message = "标识卡的物料、等级或批次与所选ASN明细不匹配" };
                    if ((line.AlreadyStockInQuantity ?? 0) + input.Quantity > line.InWarehouseQuantity)
                        return new ResponseDto { success = false,
                            message = "零头数量超过ASN剩余待入库数量；已入库物料应办理移库，不能重复新增库存" };
                    var location = await ResolveRemainderCellAsync(cellCode);
                    if (location.Cell.RunStatus != CellRunStatus.Enable || location.Cell.CellStatus == CellStatus.Full)
                        return new ResponseDto { success = false, message = "该零头柜位已禁用或标记满货" };
                    var existing = await _stockRepository.GetByCellIdAsync(location.Cell.Id);
                    if (existing.Any(x => (x.TotalCountInTime > 0 || (x.InspectionCount ?? 0) > 0) &&
                        (x.Material.MaterialCode != line.MaterialCode || x.Grade != grade)))
                        return new ResponseDto { success = false, message = "该格位已有其他物料或等级，零头格位不允许混放" };
                    location.Cell.BindRemainderMaterial(line.MaterialCode, grade);
                    var material = await _materialRepository.FindByMaterialCodeAsync(line.MaterialCode);
                    if (material == null) return new ResponseDto { success = false, message = "物料主数据不存在，请先同步物料" };
                    var stock = _stocksManager.CreateRemainderCabinetStock(input.RequestId, input.Quantity,
                        new MaterialInfoOfStock(material.MaterialCode, material.MaterialName, material.Specs, material.Unit, material.FinGoodsList),
                        new SupplierInfoOfStock(line.SupplierCode, line.SupplierName, line.BatchCode), line.BatchCode);
                    stock.BindU8AsnLine(line.Id);
                    stock.BindCell(location.Cell, location.Warehouse, location.Area);
                    stock.SetGrade(grade);
                    stock.InspectionStatus = InspectionStatus.InspectionQualified;
                    stock.StockInDate = DateTime.Now;
                    // 同一标识卡可多次用于不同批次，只保存原码供追溯，不作为唯一收料箱码。
                    stock.SetProperty("RemainderCardBarcode", input.CardBarcode.Trim());
                    stock.SetProperty("RemainderEntryId", input.RequestId.ToString("N"));
                    var history = new StockInHistory(stock.Barcode, material.MaterialCode, material.MaterialName,
                        material.Specs, material.Unit, location.Warehouse.WarehouseCode, location.Warehouse.WarehouseName,
                        location.Area.WarehouseAreaCode, location.Area.WarehouseAreaName, cellCode, location.Cell.CellName,
                        null, null, "ASN零头柜入库", input.Quantity, DateTime.Now, CurrentUser.UserName, line.BatchCode);
                    history.SetProperty("RemainderEntryId", input.RequestId.ToString("N"));
                    history.SetProperty("AsnCode", asnCode);
                    history.SetProperty("OrderCode", line.OrderCode);
                    history.SetProperty("AsnLineId", line.Id.ToString());
                    history.SetProperty("Grade", grade);
                    written = true;
                    await _stockRepository.InsertAsync(stock, autoSave: true);
                    line.ApplyAlreadyStockInQuantity(input.Quantity);
                    line.RecordRemainderEntry(input.RequestId, fingerprint);
                    await _erpAsnRepository.UpdateAsync(line, autoSave: true);
                    location.Cell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(location.Cell);
                    await _stockInHistoryRepository.InsertAsync(history);
                    await uow.CompleteAsync();
                    result = new ResponseDto { success = true, message = $"零头{input.Quantity}已计入{location.Warehouse.WarehouseName}库存，柜位{cellCode}" };
                }
            }
            catch (UserFriendlyException ex) when (!written)
            {
                return new ResponseDto { success = false, message = ex.Message };
            }
            finally { ErpAsnAppService.LocalPushLock.Release(); }
            try { result.message += await TryPushPurchaseForConfirmedLinesAsync(input.AsnCode.Trim(), new[] { input.AsnLineId }); }
            catch (Exception ex) { result.message += "，零头库存已保存，U8采购入库待处理：" + ex.Message; }
            return result;
        }

        /// <summary>整箱与零头均按本次确认的来源明细逐单采购入库；库存提交并释放本地锁后执行，不等待整张ASN。</summary>
        /// <param name="asnCode">当前送货单对应ASN号。</param>
        /// <param name="confirmedLineIds">本次已成功登记库存的ASN来源明细，其他明细不参与本次自动推送。</param>
        /// <returns>采购结果或剩余数量提示，不回滚已经保存的本地库存。</returns>
        private async Task<string> TryPushPurchaseForConfirmedLinesAsync(string asnCode, IReadOnlyCollection<Guid> confirmedLineIds)
        {
            List<ErpAsn> lines;
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                lines = (await _erpAsnRepository.GetListByAsnCodeAsync(asnCode))
                    .Where(x => x.Status != AsnStatus.Cancelled && confirmedLineIds.Contains(x.Id)).ToList();
                await read.CompleteAsync();
            }
            if (lines.Count == 0)
                return "，本地库存已保存，未找到本次单据的有效ASN来源，请核对采购入库状态";
            var message = new StringBuilder();
            foreach (var line in lines)
            {
                var source = $"订单{line.OrderCode}、物料{line.MaterialCode}、批号{line.BatchCode}";
                if (line.IsPurchasePushed)
                {
                    message.Append($"，{source}采购入库已推送，无需重复生成");
                    continue;
                }
                if (line.InWarehouseQuantity <= 0 || (line.AlreadyStockInQuantity ?? 0) != line.InWarehouseQuantity)
                {
                    message.Append($"，{source}待本单据数量与应收数量一致后推送采购入库");
                    continue;
                }
                try
                {
                    var push = await _erpAsnAppService.PushInspectionPurchaseAsync(line.Id);
                    message.Append(push?.Success == true ? $"，{source}采购入库已推送U8" :
                        $"，{source}本地库存已保存，U8采购入库待重试：" + (push?.Message ?? "接口无返回"));
                }
                catch (Exception ex) { message.Append($"，{source}本地库存已保存，U8采购入库待重试：" + ex.Message); }
            }
            return message.ToString();
        }
    }
}
