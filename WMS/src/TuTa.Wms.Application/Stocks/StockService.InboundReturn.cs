using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using Volo.Abp;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Stocks
{
    /// <summary>已有库存回入库区组盘的查询与搬运入口，复用固定库位容器及现有RCS下发流程。</summary>
    public partial class StockService
    {
        /// <summary>
        /// 按收料码中的物料编号查询库存及命中库位的整托明细，不限制条码批次；不可搬运的记录仍展示。
        /// </summary>
        public async Task<List<InboundReturnStockDto>> GetInboundReturnStocksAsync(string receivingMaterialBarcode)
        {
            var materialCode = GetInboundReturnMaterialCode(receivingMaterialBarcode);
            var stocks = await _stockRepository.GetByMaterialCodeAsync(materialCode).ConfigureAwait(false);
            var result = new List<InboundReturnStockDto>();
            // 同一库位可能命中多个批次，整托库存只查询一次，避免反复读取及重复统计。
            var cellStockCache = new Dictionary<Guid, List<StockDto>>();
            foreach (var stock in stocks.OrderBy(s => s.CellData?.CellCode).ThenBy(s => s.StockInDate))
            {
                // 库存RunStatus不参与回库判断；是否能搬运由Cells.RunStatus、绑定关系和未完成任务决定。
                if (stock.TotalCountInTime <= 0
                    || stock.CellData?.CellId.HasValue != true || stock.BoxData?.BoxId.HasValue != true
                    || string.IsNullOrWhiteSpace(stock.BoxData?.BoxCode))
                    continue;

                var cell = await _cellRepository.FindByCellCodeAsync(stock.CellData.CellCode).ConfigureAwait(false);
                if (cell == null || cell.CellType != CellType.Cell
                    || IsInboundReturnArea(cell.CellCode, stock.Warehouse?.AreaName))
                    continue;

                var hasTask = cell.RunStatus != CellRunStatus.Enable
                    || await _agvTaskManager.IsExistBoxTask(stock.BoxData.BoxCode).ConfigureAwait(false);
                if (!cellStockCache.TryGetValue(cell.Id, out var cellStocks))
                {
                    var allStocks = await _stockRepository.GetByCellIdAsync(cell.Id).ConfigureAwait(false);
                    cellStocks = allStocks.Where(s => s.TotalCountInTime > 0)
                        .OrderBy(s => s.Material?.MaterialCode).ThenBy(s => s.BatchCode).ThenBy(s => s.StockInDate)
                        .Select(s => CreateInboundReturnStockDto<StockDto>(s, cell, false)).ToList();
                    cellStockCache.Add(cell.Id, cellStocks);
                }
                var row = CreateInboundReturnStockDto<InboundReturnStockDto>(stock, cell, hasTask);
                row.CellStocks = cellStocks;
                row.CellMaterialCount = cellStocks.Select(s => s.MaterialCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code)).Distinct().Count();
                row.CellTotalCount = cellStocks.Sum(s => s.TotalCountInTime);
                row.CellTotalBoxCount = cellStocks.All(s => s.TotalPagOrBoxInTime.HasValue && s.TotalPagOrBoxInTime >= 0)
                    ? cellStocks.Sum(s => (long)s.TotalPagOrBoxInTime.Value) : (long?)null;
                result.Add(row);
            }
            return result;
        }

        /// <summary>将真实库存映射为回库展示明细；匹配行保留任务选择身份，整托明细不用于替换所选物料。</summary>
        /// <typeparam name="TStockDto">命中库存或整托库存明细的返回类型。</typeparam>
        /// <param name="stock">原库存记录，数量、箱数及批次均不使用扫码内容覆盖。</param>
        /// <param name="cell">库存所在的实际库位。</param>
        /// <param name="hasTask">该命中库存是否因库位或活动任务而禁止选择。</param>
        /// <returns>包含物料、库位、容器、包装及批次信息的展示记录。</returns>
        private static TStockDto CreateInboundReturnStockDto<TStockDto>(Stock stock, Cell cell, bool hasTask)
            where TStockDto : StockDto, new() => new TStockDto
        {
            Id = stock.Id,
            Barcode = stock.Barcode,
            ReceivingMaterialBarcode = stock.ReceivingMaterialBarcode,
            CellId = cell.Id,
            CellCode = cell.CellCode,
            CellName = cell.CellName,
            BoxId = stock.BoxData?.BoxId,
            BoxCode = stock.BoxData?.BoxCode,
            BoxName = stock.BoxData?.BoxName,
            BoxNumber = stock.BoxData?.BoxNumber,
            HouseId = cell.WarehouseId,
            HouseCode = stock.Warehouse?.HouseCode,
            HouseName = stock.Warehouse?.HouseName,
            AreaName = stock.Warehouse?.AreaName,
            MaterialCode = stock.Material?.MaterialCode,
            MaterialName = stock.Material?.MaterialName,
            Specs = stock.Material?.Specs,
            Unit = stock.Material?.Unit,
            TotalCountInTime = stock.TotalCountInTime,
            TotalPagOrBoxInTime = stock.TotalPagOrBox,
            CountInOnePkgOrBox = stock.ReceiveCount?.CountInOnePkgOrBox,
            BatchCode = stock.BatchCode,
            ProcessNo = stock.ProcessNo,
            Grade = stock.Grade,
            Status = stock.Status.ToString(),
            RunStatus = stock.RunStatus.ToString(),
            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
            HasTask = hasTask
        };

        /// <summary>
        /// 校验人工选择和目的地后，按AGV配置的模板创建整盘回库任务；下发期间锁定起终点，完成回调再迁移库存。
        /// </summary>
        [UnitOfWork]
        public async Task<ResponseDto> CreateInboundReturnTaskAsync(InboundReturnTaskCreateDto input)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    if (input == null || input.StockId == Guid.Empty
                        || string.IsNullOrWhiteSpace(input.SourceCellCode)
                        || string.IsNullOrWhiteSpace(input.BoxCode)
                        || string.IsNullOrWhiteSpace(input.EndCellCode))
                        throw new UserFriendlyException("请选择库存并输入目标入库库位");

                    var materialCode = GetInboundReturnMaterialCode(input.ReceivingMaterialBarcode);
                    var sourceCode = input.SourceCellCode.Trim();
                    var targetCode = input.EndCellCode.Trim();
                    var boxCode = input.BoxCode.Trim();
                    if (sourceCode == targetCode)
                        throw new UserFriendlyException("起始库位与目标入库库位不能相同");

                    var stock = await _stockRepository.FindAsync(input.StockId).ConfigureAwait(false);
                    // 与查询入口一致，不检查库存RunStatus；下方使用实际库位的RunStatus校验可搬运性。
                    if (stock == null || stock.Material?.MaterialCode != materialCode
                        || stock.TotalCountInTime <= 0)
                        throw new UserFriendlyException("所选库存不存在、物料不匹配或已不可搬运，请重新查询");
                    if (stock.CellData?.CellCode != sourceCode || stock.BoxData?.BoxCode != boxCode)
                        throw new UserFriendlyException("所选库存的库位或容器已改变，请重新查询");

                    var source = await _cellRepository.FindByCellCodeAsync(sourceCode).ConfigureAwait(false);
                    var target = await _cellRepository.FindByCellCodeAsync(targetCode).ConfigureAwait(false);
                    if (source == null || source.CellType != CellType.Cell || source.RunStatus != CellRunStatus.Enable
                        || stock.CellData.CellId != source.Id || IsInboundReturnArea(sourceCode, stock.Warehouse?.AreaName))
                        throw new UserFriendlyException("来源库位不属于可搬运的仓库库位，或已被其他任务占用");
                    if (target == null)
                        throw new UserFriendlyException($"目标入库库位{targetCode}不存在");
                    if (target.WarehouseId != source.WarehouseId)
                        throw new UserFriendlyException("目标入库库位与来源库位必须属于同一仓库");
                    if (target.CellType != CellType.Cell || target.RunStatus != CellRunStatus.Enable
                        || target.CellStatus != CellStatus.Nohave)
                        throw new UserFriendlyException($"目标入库库位{targetCode}不是空闲货位");

                    var warehouse = await _warehouseRepository.FindByIdAsync(target.WarehouseId).ConfigureAwait(false);
                    var area = target.WarehouseAreaId.HasValue
                        ? warehouse?.GetAreaByAreaId(target.WarehouseAreaId.Value) : null;
                    if (area == null || !IsInboundReturnArea(targetCode, area.WarehouseAreaName))
                        throw new UserFriendlyException($"目标库位{targetCode}不属于入库区或未配置所属库区");

                    var sourceBox = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (sourceBox == null || sourceBox.Id != stock.BoxData.BoxId || sourceBox.CellData?.CellId != source.Id)
                        throw new UserFriendlyException("来源容器与所选库位不匹配，请重新查询");

                    // 当前系统以库位固定容器承接库存，提前核对目标容器，避免搬运完成后回调无法入账。
                    var targetBox = await _boxRepository.FindByCellIdAsync(target.Id).ConfigureAwait(false)
                        ?? await _boxRepository.FindByBoxCodeAsync(targetCode).ConfigureAwait(false);
                    if (targetBox == null || targetBox.Id == sourceBox.Id
                        || (targetBox.CellData?.CellId.HasValue == true && targetBox.CellData.CellId != target.Id))
                        throw new UserFriendlyException($"目标入库库位{targetCode}未配置有效的固定容器");
                    var targetStocks = await _stockRepository.GetByCellIdAsync(target.Id).ConfigureAwait(false);
                    var targetBoxStocks = await _stockRepository.GetByBoxIdAsync(targetBox.Id).ConfigureAwait(false);
                    if (targetStocks.Count > 0 || targetBoxStocks.Count > 0)
                        throw new UserFriendlyException($"目标入库库位{targetCode}或对应容器已有库存");
                    if (await _agvTaskManager.IsExistBoxTask(targetBox.BoxCode).ConfigureAwait(false))
                        throw new UserFriendlyException($"目标入库库位{targetCode}的容器已有未完成任务");

                    // 回到4A后还要进行人工组盘，沿用现有巷道占用规则。
                    var laneResult = await Validate4ALaneGroupingOrderAsync(target).ConfigureAwait(false);
                    if (laneResult != null && !laneResult.success)
                        return laneResult;

                    // 独立读取回库模板，避免影响普通入库、出库及库存整理任务；兼容未配置或空配置。
                    var taskTemplate = string.IsNullOrWhiteSpace(_aGVOptions.StockReturnToInboundTaskType)
                        ? "De04" : _aGVOptions.StockReturnToInboundTaskType.Trim();
                    var result = await CreateStockTaskV2CoreAsync(boxCode, sourceCode, targetCode,
                        ManageType.StockReturnToInbound, taskTemplate).ConfigureAwait(false);
                    if (result.success)
                    {
                        await uow.CompleteAsync().ConfigureAwait(false);
                        result.message = $"回库搬运任务已下发，目标入库库位：{targetCode}";
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error($"创建回库组盘任务失败：{ex.Message}");
                    return new ResponseDto { success = false, message = ex.Message };
                }
            }
        }

        /// <summary>解析新6段及旧8段收料码，仅使用第一段物料编号检索不同批次的已有库存。</summary>
        private static string GetInboundReturnMaterialCode(string receivingMaterialBarcode)
        {
            var parts = receivingMaterialBarcode?.Trim().Split(',');
            if (parts == null || (parts.Length != 6 && parts.Length != 8) || string.IsNullOrWhiteSpace(parts[0]))
                throw new UserFriendlyException("收料条形码格式错误，请扫描以英文逗号分隔的6段或8段完整收料码");
            return parts[0].Trim();
        }

        /// <summary>优先识别入库区业务名称，同时兼容项目既有4A入库库位编码。</summary>
        private static bool IsInboundReturnArea(string cellCode, string areaName) =>
            areaName == "入库区" || (cellCode?.StartsWith("4A", StringComparison.OrdinalIgnoreCase) == true);
    }
}
