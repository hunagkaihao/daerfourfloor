using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeChecks.Aggregates;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Boxes.Entities;
using TuTa.Wms.Boxes.Events;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.Domain;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Moves;
using TuTa.Wms.Moves.Aggregates;
using TuTa.Wms.PickLists;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Entities;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Entities;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.Skips.Dtos;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using TuTa.Wms.Stocks.Events;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Warehouses;
using TuTa.Wms.StockOutHistories;
using TuTa.Wms.StockOutHistories.Aggregates;
using TuTa.Wms.StockInHistories;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Events;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Wms.LogTool;
using TuTa.Wms.Boxes;

namespace TuTa.Wms.Stocks
{
    public class StockService : WmsAppService, IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly StocksManager _stocksManager;
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IRepository<BoxStock> _boxStockRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly ISkipRepository _skipRepository;
        private readonly IBarcodeCheckRepository _barcodeCheckRepository;
        private readonly IRecheckItemRepository _recheckItemRepository;
        private readonly IPickListRepository _pickListRepository;
        private readonly IPickItemRepository _pickItemRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IMoveRepository _moveRepository;
        private readonly IStockOutHistoryRepository _stockOutHistoryRepository;
        private readonly IStockInHistoryRepository _stockInHistoryRepository;
        private readonly IErpAsnStockInService _erpAsnStockInService;
        private readonly IErpAsnRepository _erpAsnRepository;
        private readonly IErpAsnAppService _erpAsnAppService;
        private readonly CellManager _cellManager;
        private readonly AgvTaskManager _agvTaskManager;
        private readonly BoxManager _boxManager;
        private readonly IdentityUserManager _userManager;
        private readonly LocalEventBus _localEventBus;
        private readonly ILogger<StockService> _logger;
        private readonly AGVOptions _aGVOptions;


        public StockService(
            IStockRepository stockRepository,
            StocksManager stocksManager,
            IChkResultListRepository chkResultListRepository,
            IBoxRepository boxRepository,
            IRepository<BoxStock> boxStockRepository,
            ICellRepository cellRepository,
            IWarehouseRepository warehouseRepository,
            IBarcodeListRepository barcodeListRepository,
            ISkipRepository skipRepository,
            IBarcodeCheckRepository barcodeCheckRepository,
            IRecheckItemRepository recheckItemRepository,
            IPickListRepository pickListRepository,
            IPickItemRepository pickItemRepository,
            IMaterialRepository materialRepository,
            IMoveRepository moveRepository,
            IStockOutHistoryRepository stockOutHistoryRepository,
            IStockInHistoryRepository stockInHistoryRepository,
            IErpAsnStockInService erpAsnStockInService,
            IErpAsnRepository erpAsnRepository,
            IErpAsnAppService erpAsnAppService,

            CellManager cellManager,
            AgvTaskManager agvTaskManager,
            BoxManager boxManager,
            IdentityUserManager userManager,
            LocalEventBus localEventBus,
            ILogger<StockService> logger,
            IOptionsSnapshot<AGVOptions> aGVOptions)
        {
            _stockRepository = stockRepository;
            _stocksManager = stocksManager;
            _chkResultListRepository = chkResultListRepository;
            _boxRepository = boxRepository;
            _boxStockRepository = boxStockRepository;
            _cellRepository = cellRepository;
            _warehouseRepository = warehouseRepository;
            _barcodeListRepository = barcodeListRepository;
            _skipRepository = skipRepository;
            _barcodeCheckRepository = barcodeCheckRepository;
            _pickListRepository = pickListRepository;
            _pickItemRepository = pickItemRepository;
            _recheckItemRepository = recheckItemRepository;
            _materialRepository = materialRepository;
            _moveRepository = moveRepository;
            _stockOutHistoryRepository = stockOutHistoryRepository;
            _stockInHistoryRepository = stockInHistoryRepository;
            _erpAsnStockInService = erpAsnStockInService;
            _erpAsnRepository = erpAsnRepository;
            _erpAsnAppService = erpAsnAppService;
            _agvTaskManager = agvTaskManager;
            _cellManager = cellManager;
            _boxManager = boxManager;
            _userManager = userManager;
            _localEventBus = localEventBus;
            _logger = logger;
            _aGVOptions = aGVOptions.Value;
        }
        

        //容器组盘
        [UnitOfWork]
        public async Task<ResponseDto> CreateStockAndBindBoxAsync(List<StockCreateDto> paras, string boxCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var result = await CreateStockAndBindBoxCoreAsync(paras, boxCode).ConfigureAwait(false);
                    if (result.success)
                    {
                        await uow.CompleteAsync().ConfigureAwait(false);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return new ResponseDto() { success = false, message = ex.Message };
                }
            }
        }

        /// <summary>
        /// 容器组盘（关联ASN订单号，更新已入库数量）
        /// </summary>
        [UnitOfWork]
        public async Task<ResponseDto> CreateStockAndBindBoxWithAsnAsync(List<StockCreateDto> paras, string boxCode, string orderCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var validateResult = await ValidateAsnOrderStockInAsync(orderCode, paras).ConfigureAwait(false);
                    if (!validateResult.success)
                    {
                        return validateResult;
                    }

                    var result = await CreateStockAndBindBoxCoreAsync(paras, boxCode).ConfigureAwait(false);
                    if (!result.success)
                    {
                        return result;
                    }

                    await UpdateAsnOrderStockInAsync(orderCode, paras).ConfigureAwait(false);

                    var asnCode = await GetAsnCodeByOrderAndMaterialAsync(orderCode, paras).ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(asnCode))
                    {
                        var pushResult = await _erpAsnAppService.TryPushPuArrVouchIfAllLinesCompletedAsync(asnCode).ConfigureAwait(false);
                        if (pushResult != null)
                        {
                            if (pushResult.Success)
                            {
                                result.message = string.IsNullOrWhiteSpace(pushResult.Message)
                                    ? result.message + "，已自动推送U8到货单"
                                    : result.message + "，" + pushResult.Message;
                            }
                            else
                            {
                                _logger.Warn($"ASN {asnCode} 自动推送到货单失败：{pushResult.Message}，开始回滚ASN已入库数量和状态");
                                await RevertAsnOrderStockInAsync(orderCode, paras).ConfigureAwait(false);
                            }
                        }
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return new ResponseDto() { success = false, message = ex.Message };
                }
            }
        }

        private async Task<ResponseDto> CreateStockAndBindBoxCoreAsync(List<StockCreateDto> paras, string boxCode)
        {
            if (string.IsNullOrWhiteSpace(boxCode))
                return new ResponseDto() { success = false, message = "容器编号不能为空" };

            // 按容器编号查找容器
            var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
            Cell cell = null;

            if (box == null)
            {
                // 容器不存在，使用传入的容器编号创建
                var boxSpecs = new TuTa.Wms.Boxes.ValueObjects.BoxSpecsValObj("默认规格", 1, 1, 1);
                box = await _boxManager.CreateBoxAsync(boxCode, "容器" + boxCode, "1", boxSpecs).ConfigureAwait(false);
                await _boxRepository.InsertAsync(box).ConfigureAwait(false);
            }

            // 解析库位：优先从容器的库位绑定获取，否则按同编号查找库位
            if (box.CellData?.CellId != null)
            {
                cell = await _cellRepository.FindByIdAsync(box.CellData.CellId.Value).ConfigureAwait(false);
            }
            else
            {
                cell = await _cellRepository.FindByCellCodeAsync(boxCode).ConfigureAwait(false);
            }

            if (cell == null)
                return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器未绑定库位，且找不到对应库位" };

            if (boxCode.StartsWith("4A", StringComparison.OrdinalIgnoreCase))
            {
                var laneValidation = await Validate4ALaneGroupingOrderAsync(cell).ConfigureAwait(false);
                if (laneValidation != null)
                    return laneValidation;
            }

            // 容器未绑定库位时，绑定容器到库位
            if (box.CellData?.CellId == null)
            {
                if (cell.CellStatus != CellStatus.Nohave && !cell.IsBoxInThisCell(box.Id))
                    return new ResponseDto() { success = false, message = $"{cell.CellCode}库位已有容器绑定" };

                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);
                box.BindCell(cell, warehouse, warehouseArea);
                await _boxRepository.UpdateAsync(box).ConfigureAwait(false);

                cell.SetCellStatus(CellStatus.Have);
                await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
            }

            // 检查参数
            if (paras == null || paras.Count == 0)
                return new ResponseDto() { success = false, message = "库存创建参数不能为空" };

            foreach (var para in paras)
            {
                var materialCode = !string.IsNullOrWhiteSpace(para.MaterialCode) ? para.MaterialCode : para.Barcode;
                if (string.IsNullOrWhiteSpace(materialCode))
                    return new ResponseDto() { success = false, message = "物料码不能为空" };

                if (para.TotalCount <= 0)
                    return new ResponseDto() { success = false, message = "库存数量必须大于0" };

                // 查找收料条码
                /*BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(para.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    return new ResponseDto() { success = false, message = $"收料码为{para.Barcode}的收料条目不存在" };*/

                // 查找物料信息
                Material materialResult = await _materialRepository.FindByMaterialCodeAsync(materialCode);
                if (materialResult == null)
                    return new ResponseDto() { success = false, message = $"物料码为{materialCode}的物料数据不存在" };

                // 创建库存信息
                MaterialInfoOfStock material = new MaterialInfoOfStock(
                    materialResult.MaterialCode,
                    materialResult.MaterialName,
                    materialResult.Specs,
                    materialResult.Unit,
                    materialResult.FinGoodsList);

                CountInfoOfStock countInfo = new CountInfoOfStock(
                    para.TotalCount,
                    para.ReceivePkgOrBoxCount,
                    para.CountInOnePkgOrBox);

                SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                    null,
                    null,
                    para.SupplierBatchCode);

                // 创建库存，传入批次、等级等信息
                var stock = await _stocksManager.CreateStockAsync(
                    materialCode,
                    para.TotalCount,
                    material,
                    countInfo,
                    supplierInfo,
                    StockInType.AdjustStockIn,
                    2,
                    para.BatchCode,
                    null,
                    null);

                // 设置生产日期
                if (para.SupplierProductionDate.HasValue)
                {
                    stock.UpdateStockInDate(para.SupplierProductionDate.Value);
                }

                // 设置等级
                if (!string.IsNullOrWhiteSpace(para.Grade))
                {
                    stock.SetGrade(para.Grade);
                }

                // 设置工序号
                if (!string.IsNullOrWhiteSpace(para.ProcessNo))
                {
                    stock.SetProcessNo(para.ProcessNo);
                }

                // 设置收料条形码
                if (!string.IsNullOrWhiteSpace(para.ReceivingMaterialBarcode))
                {
                    var existingStockByReceivingBarcode = await _stockRepository.FindByReceivingMaterialBarcodeAsync(para.ReceivingMaterialBarcode).ConfigureAwait(false);
                    if (existingStockByReceivingBarcode != null)
                    {
                        existingStockByReceivingBarcode.CombineStock(para.TotalCount);
                        await _stockRepository.UpdateAsync(existingStockByReceivingBarcode).ConfigureAwait(false);

                        if (cell != null)
                        {
                            Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                            WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);

                            var stockInHistory = new StockInHistory(
                                existingStockByReceivingBarcode.Barcode,
                                materialResult.MaterialCode,
                                materialResult.MaterialName,
                                materialResult.Specs,
                                materialResult.Unit,
                                warehouse.WarehouseCode,
                                warehouse.WarehouseName,
                                warehouseArea.WarehouseAreaCode,
                                warehouseArea.WarehouseAreaName,
                                cell.CellCode,
                                cell.CellName,
                                box.BoxCode,
                                box.BoxName,
                                StockInTypeHelper.StockInTypeToChinese(StockInType.AdjustStockIn),
                                para.TotalCount,
                                DateTime.Now,
                                batchNo: para.BatchCode);

                            await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);

                            if (!string.IsNullOrWhiteSpace(para.AsnCode))
                            {
                                await _erpAsnStockInService.HandleStockInCompletedAsync(para.AsnCode, para.TotalCount).ConfigureAwait(false);
                            }
                        }

                        continue;
                    }

                    stock.SetReceivingMaterialBarcode(para.ReceivingMaterialBarcode);
                }
 
                // 检查是否已存在相同库存
                var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, materialCode).ConfigureAwait(false);
                if (stockExist != null)
                {
                    // 合并库存
                    stockExist.CombineStock(stock);
                    await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                }
                else
                {
                    // 绑定库存到容器
                    stock.BindBox(box.Id, box.BoxCode, box.BoxName, para.BoxNumber);
                            
                    if (cell != null)
                    {
                        Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                        WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);
                        stock.BindCell(cell, warehouse, warehouseArea);

                        var stockInHistory = new StockInHistory(
                            stock.Barcode,
                            materialResult.MaterialCode,
                            materialResult.MaterialName,
                            materialResult.Specs,
                            materialResult.Unit,
                            warehouse.WarehouseCode,
                            warehouse.WarehouseName,
                            warehouseArea.WarehouseAreaCode,
                            warehouseArea.WarehouseAreaName,
                            cell.CellCode,
                            cell.CellName,
                            box.BoxCode,
                            box.BoxName,
                            StockInTypeHelper.StockInTypeToChinese(StockInType.AdjustStockIn),
                            para.TotalCount,
                            DateTime.Now,
                            batchNo: para.BatchCode);

                        await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(para.AsnCode))
                        {
                            await _erpAsnStockInService.HandleStockInCompletedAsync(para.AsnCode, para.TotalCount).ConfigureAwait(false);
                        }
                    }
                            
                    await _stockRepository.InsertAsync(stock).ConfigureAwait(false);
                }
            }

            // 创建库存后，更新容器状态为有货
            box.SetHave();
            await _boxRepository.UpdateAsync(box).ConfigureAwait(false);

            if (cell != null && cell.CellStatus != CellStatus.Have)
            {
                cell.SetCellStatus(CellStatus.Have);
                await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
            }

            return new ResponseDto() { success = true, message = "创建库存并绑定容器成功" };
        }

        private async Task<ResponseDto> ValidateAsnOrderStockInAsync(string orderCode, List<StockCreateDto> paras)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return new ResponseDto() { success = false, message = "订单号不能为空" };
            }

            if (paras == null || paras.Count == 0)
            {
                return new ResponseDto() { success = false, message = "库存创建参数不能为空" };
            }

            var quantityByMaterial = GetStockInQuantityByMaterial(paras);
            foreach (var item in quantityByMaterial)
            {
                var erpAsn = await _erpAsnRepository.GetByOrderCodeAndMaterialCodeAsync(orderCode, item.Key).ConfigureAwait(false);
                if (erpAsn == null)
                {
                    return new ResponseDto()
                    {
                        success = false,
                        message = $"未找到订单号{orderCode}、物料{item.Key}对应的ASN单据"
                    };
                }

                if (erpAsn.Status == AsnStatus.Completed)
                {
                    return new ResponseDto()
                    {
                        success = false,
                        message = $"订单号{orderCode}、物料{item.Key}对应的ASN单据已完成"
                    };
                }

                var current = erpAsn.AlreadyStockInQuantity ?? 0;
                if (current + item.Value > erpAsn.InWarehouseQuantity)
                {
                    return new ResponseDto()
                    {
                        success = false,
                        message = $"订单号{orderCode}本次组盘数量{item.Value}加上已入库数{current}不能超过入库数{erpAsn.InWarehouseQuantity}"
                    };
                }
            }

            return new ResponseDto() { success = true };
        }

        private async Task UpdateAsnOrderStockInAsync(string orderCode, List<StockCreateDto> paras)
        {
            var quantityByMaterial = GetStockInQuantityByMaterial(paras);
            foreach (var item in quantityByMaterial)
            {
                var erpAsn = await _erpAsnRepository.GetByOrderCodeAndMaterialCodeAsync(orderCode, item.Key).ConfigureAwait(false);
                if (erpAsn == null)
                {
                    throw new BusinessException($"未找到订单号{orderCode}、物料{item.Key}对应的ASN单据");
                }

                erpAsn.ApplyAlreadyStockInQuantity(item.Value);
                await _erpAsnRepository.UpdateAsync(erpAsn).ConfigureAwait(false);
            }
        }

        private async Task RevertAsnOrderStockInAsync(string orderCode, List<StockCreateDto> paras)
        {
            var quantityByMaterial = GetStockInQuantityByMaterial(paras);
            foreach (var item in quantityByMaterial)
            {
                var erpAsn = await _erpAsnRepository.GetByOrderCodeAndMaterialCodeAsync(orderCode, item.Key).ConfigureAwait(false);
                if (erpAsn == null)
                {
                    _logger.Warn($"回滚ASN失败：未找到订单号{orderCode}、物料{item.Key}对应的ASN单据");
                    continue;
                }

                erpAsn.RevertAlreadyStockInQuantity(item.Value);
                await _erpAsnRepository.UpdateAsync(erpAsn).ConfigureAwait(false);
                _logger.Warn($"已回滚ASN {erpAsn.AsnCode} 物料 {item.Key} 已入库数量 {item.Value}");
            }
        }

        private async Task<string> GetAsnCodeByOrderAndMaterialAsync(string orderCode, List<StockCreateDto> paras)
        {
            var quantityByMaterial = GetStockInQuantityByMaterial(paras);
            if (quantityByMaterial.Count == 0)
            {
                return null;
            }

            var firstMaterial = quantityByMaterial.Keys.First();
            var erpAsn = await _erpAsnRepository.GetByOrderCodeAndMaterialCodeAsync(orderCode, firstMaterial).ConfigureAwait(false);
            return erpAsn?.AsnCode;
        }

        private static Dictionary<string, decimal> GetStockInQuantityByMaterial(List<StockCreateDto> paras)
        {
            return paras
                .GroupBy(p => !string.IsNullOrWhiteSpace(p.MaterialCode) ? p.MaterialCode : p.Barcode)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.TotalCount));
        }
        //创建容器搬运任务
        [UnitOfWork]
        public async Task<ResponseDto> CreateStockTask(string boxCode, string startCellCode, string endCellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                    {
                        _logger.Warn($"创建搬运任务失败：容器{boxCode}不存在");
                        return new ResponseDto() { success = false, message = $"容器{boxCode}不存在" };
                    }

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                    {
                        _logger.Warn($"创建搬运任务失败：容器{boxCode}已有未完成任务");
                        return new ResponseDto() { success = false, message = $"容器{boxCode}已有未完成任务" };
                    }

                    var startCell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (startCell == null)
                    {
                        _logger.Warn($"创建搬运任务失败：起始库位{startCellCode}不存在");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}不存在" };
                    }

                    Cell endCell = null;

                    if (endCellCode == null || endCellCode == "")
                    {
                        var availableCells = await _cellRepository.GetListAsync(
                            f => f.CellCode.StartsWith("4F")
                                 && f.CellStatus == CellStatus.Nohave
                                 && f.RunStatus == CellRunStatus.Enable).ConfigureAwait(false);
                        endCell = SelectEndCellBy4FOrdering(availableCells);
                        if (endCell == null)
                        {
                            _logger.Warn($"创建搬运任务失败：4F区域无可用空库位(共检查{availableCells.Count}个)");
                            return new ResponseDto() { success = false, message = "4F区域无可用空库位" };
                        }
                    }
                    else
                    {
                        endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                        if (endCell == null)
                        {
                            _logger.Warn($"创建搬运任务失败：目标库位{endCellCode}不存在");
                            return new ResponseDto() { success = false, message = $"目标库位{endCellCode}不存在" };
                        }
                    }

                    if (startCell.RunStatus != CellRunStatus.Enable)
                    {
                        _logger.Warn($"创建搬运任务失败：起始库位{startCellCode}运行状态为{startCell.RunStatus}，不可用");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}当前不可用" };
                    }

                    if (startCell.CellType != CellType.Cell)
                    {
                        _logger.Warn($"创建搬运任务失败：起始库位{startCellCode}类型为{startCell.CellType}，不允许搬运");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}类型不允许搬运" };
                    }

                    // 巷道校验：外侧(更大的LanePosition)是否有已组盘但未创建任务的库位
                    if (!string.IsNullOrWhiteSpace(startCell.LaneToColumn) && startCell.LanePosition.HasValue)
                    {
                        var outerCells = await _cellRepository.GetListAsync(c =>
                            c.LaneToColumn == startCell.LaneToColumn
                            && c.LanePosition > startCell.LanePosition.Value
                            && c.CellStatus != CellStatus.Nohave
                            && c.RunStatus != CellRunStatus.Selected).ConfigureAwait(false);
                        if (outerCells.Count > 0)
                        {
                            var outerCellCodes = string.Join("、", outerCells.OrderBy(c => c.LanePosition).Select(c => c.CellCode));
                            _logger.Warn($"创建搬运任务失败：外侧库位({outerCellCodes})已组盘，请先创建外侧库位的搬运任务");
                            return new ResponseDto() { success = false, message = $"外侧库位({outerCellCodes})已组盘，请先将外侧库位的搬运任务创建后再创建库位{startCellCode}的任务" };
                        }
                    }

                    var dispatchToRcs = true;
                    if (startCell.CellCode.StartsWith("4A", StringComparison.OrdinalIgnoreCase))
                    {
                        var laneCheck = await Evaluate4AInboundLaneAsync(startCell).ConfigureAwait(false);
                        if (!laneCheck.CanCreate)
                        {
                            _logger.Warn($"创建搬运任务失败：4A库位{startCellCode}巷道校验不通过");
                            return new ResponseDto() { success = false, message = "4A库位巷道校验不通过" };
                        }
                        dispatchToRcs = laneCheck.DispatchToRcs;
                    }
                    //if (!dispatchToRcs)
                    //{
                    //    _logger.Warn($"创建搬运任务失败：4A库位{startCellCode}外侧库位任务未完成，无法下发给RCS");
                    //    return new ResponseDto() { success = false, message = "外侧库位任务仍在执行中，请等待外侧库位任务完成后再创建" };
                    //}

                    _logger.Info($"开始创建agv任务，容器:{boxCode}，起点:{startCellCode}，终点:{endCell.CellCode}");

                    // 根据容器类型选择合适的任务类型
                    ManageType taskType = ManageType.CTUStockIn;
                    if (box.BoxTypeName == "1")
                    {
                        taskType = ManageType.CTUStockOut;
                    }
                    else if (box.BoxTypeName == "2")
                    {
                        taskType = ManageType.LiftStockOut;
                    }

                    await SetAsExecutingAsync(startCell, endCell, null, box, taskType, dispatchToRcs, _aGVOptions.CreateStockTaskType).ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                    _logger.Info($"AGV任务创建成功，容器:{boxCode}，任务类型:{taskType}");
                    return new ResponseDto() { success = true, message = "任务创建成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"创建搬运任务异常：{ex.Message}\n{ex.StackTrace}");
                    return new ResponseDto() { success = false, message = $"任务创建异常：{ex.Message}" };
                }
            }
        }

        /// <summary>
        /// 创建容器搬运任务（无4A巷道校验，统一使用De02模板）
        /// </summary>
        [UnitOfWork]
        public async Task<ResponseDto> CreateStockTaskV2(string boxCode, string startCellCode, string endCellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                    {
                        _logger.Warn($"创建搬运任务V2失败：容器{boxCode}不存在");
                        return new ResponseDto() { success = false, message = $"容器{boxCode}不存在" };
                    }

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                    {
                        _logger.Warn($"创建搬运任务V2失败：容器{boxCode}已有未完成任务");
                        return new ResponseDto() { success = false, message = $"容器{boxCode}已有未完成任务" };
                    }

                    var startCell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (startCell == null)
                    {
                        _logger.Warn($"创建搬运任务V2失败：起始库位{startCellCode}不存在");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}不存在" };
                    }

                    Cell endCell = null;

                    if (endCellCode == null || endCellCode == "")
                    {
                        var availableCells = await _cellRepository.GetListAsync(
                            f => f.CellCode.StartsWith("4F")
                                 && f.CellStatus == CellStatus.Nohave
                                 && f.RunStatus == CellRunStatus.Enable).ConfigureAwait(false);
                        endCell = SelectEndCellBy4FOrdering(availableCells);
                        if (endCell == null)
                        {
                            _logger.Warn($"创建搬运任务V2失败：4F区域无可用空库位(共检查{availableCells.Count}个)");
                            return new ResponseDto() { success = false, message = "4F区域无可用空库位" };
                        }
                    }
                    else
                    {
                        endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                        if (endCell == null)
                        {
                            _logger.Warn($"创建搬运任务V2失败：目标库位{endCellCode}不存在");
                            return new ResponseDto() { success = false, message = $"目标库位{endCellCode}不存在" };
                        }
                    }

                    if (startCell.RunStatus != CellRunStatus.Enable)
                    {
                        _logger.Warn($"创建搬运任务V2失败：起始库位{startCellCode}运行状态为{startCell.RunStatus}，不可用");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}当前不可用" };
                    }

                    if (startCell.CellType != CellType.Cell)
                    {
                        _logger.Warn($"创建搬运任务V2失败：起始库位{startCellCode}类型为{startCell.CellType}，不允许搬运");
                        return new ResponseDto() { success = false, message = $"起始库位{startCellCode}类型不允许搬运" };
                    }

                    // 巷道校验：外侧(更大的LanePosition)是否有已组盘但未创建任务的库位
                    if (!string.IsNullOrWhiteSpace(startCell.LaneToColumn) && startCell.LanePosition.HasValue)
                    {
                        var outerCells = await _cellRepository.GetListAsync(c =>
                            c.LaneToColumn == startCell.LaneToColumn
                            && c.LanePosition > startCell.LanePosition.Value
                            && c.CellStatus != CellStatus.Nohave
                            && c.RunStatus != CellRunStatus.Selected).ConfigureAwait(false);
                        if (outerCells.Count > 0)
                        {
                            var outerCellCodes = string.Join("、", outerCells.OrderBy(c => c.LanePosition).Select(c => c.CellCode));
                            _logger.Warn($"创建搬运任务V2失败：外侧库位({outerCellCodes})已组盘，请先创建外侧库位的搬运任务");
                            return new ResponseDto() { success = false, message = $"外侧库位({outerCellCodes})已组盘，请先将外侧库位的搬运任务创建后再创建库位{startCellCode}的任务" };
                        }
                    }

                    _logger.Info($"开始创建agv任务(V2)，容器:{boxCode}，起点:{startCellCode}，终点:{endCell?.CellCode ?? "自动分配"}");

                    ManageType taskType = ManageType.CTUStockIn;
                    if (box.BoxTypeName == "1")
                    {
                        taskType = ManageType.CTUStockOut;
                    }
                    else if (box.BoxTypeName == "2")
                    {
                        taskType = ManageType.LiftStockOut;
                    }

                    var taskTypOverride = _aGVOptions.CreateStockOutTaskType ?? "De02";
                    await SetAsExecutingAsync(startCell, endCell, null, box, taskType, true, taskTypOverride).ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                    _logger.Info($"AGV任务V2创建成功，容器:{boxCode}，任务类型:{taskType}");
                    return new ResponseDto() { success = true, message = "任务创建成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"创建搬运任务V2异常：{ex.Message}\n{ex.StackTrace}");
                    return new ResponseDto() { success = false, message = $"任务创建异常：{ex.Message}" };
                }
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> StockDisBindBox(string boxCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    // 查找容器
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    // 获取容器中的所有库存
                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(box.Id);
                    if (stocks == null || stocks.Count == 0)
                        return new ResponseDto() { success = false, message = "容器中没有库存" };

                    Guid? cellId = box.CellData.CellId;

                    // 在删除前查询库位中库存，用于判断是否需要重置库位状态
                    List<Stock> stocksInCellBeforeDelete = null;
                    if (cellId.HasValue)
                    {
                        stocksInCellBeforeDelete = await _stockRepository.GetByCellIdAsync(cellId.Value).ConfigureAwait(false);
                    }

                    // 从容器中移除所有库存关联
                    foreach (Stock stock in stocks)
                    {
                        box.RemoveStock(stock.Id);
                    }

                    // 删除所有BoxStock中间表记录
                    await _boxStockRepository.DeleteAsync(bs => bs.BoxId == box.Id);

                    // 更新容器状态
                    await _boxRepository.UpdateAsync(box);

                    // 删除所有库存
                    foreach (Stock stock in stocks)
                    {
                        await _stockRepository.DeleteAsync(stock);
                    }

                    // 重置关联的ASN已入库数量
                    foreach (Stock stock in stocks)
                    {
                        var orderCode = stock.BatchCode;
                        var materialCode = stock.Material?.MaterialCode;
                        if (!string.IsNullOrEmpty(orderCode) && !string.IsNullOrEmpty(materialCode))
                        {
                            var erpAsn = await _erpAsnRepository.GetByOrderCodeAndMaterialCodeAsync(orderCode, materialCode).ConfigureAwait(false);
                            if (erpAsn != null)
                            {
                                erpAsn.SetAlreadyStockInQuantity(null);
                                await _erpAsnRepository.UpdateAsync(erpAsn).ConfigureAwait(false);
                            }
                        }
                    }

                    // 若容器中库存就是库位中全部库存，解绑后库位变为无货
                    if (stocksInCellBeforeDelete != null && stocksInCellBeforeDelete.Count == stocks.Count)
                    {
                        var cell = await _cellRepository.FindByIdAsync(cellId.Value).ConfigureAwait(false);
                        if (cell != null)
                        {
                            cell.SetCellStatus(CellStatus.Nohave);
                            await _cellRepository.UpdateAsync(cell);
                        }
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "库存解绑容器成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> BindCellAsync(string boxCode, string cellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    // 查找容器
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    // 查找库位
                    var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位不存在" };

                    // 检查容器是否已经绑定库位
                    if (box.CellData.CellCode != null)
                        return new ResponseDto() { success = false, message = $"容器已绑定{box.CellData.CellCode}库位" };

                    // 检查库位是否已有容器
                    if (cell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"{cell.CellCode}库位已有容器绑定" };

                    // 获取仓库和库区信息
                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);

                    // 绑定容器到库位
                    box.BindCell(cell, warehouse, warehouseArea);
                    await _boxRepository.UpdateAsync(box);

                    // 更新库位状态
                    cell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(cell);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "容器绑定库位成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> DisBindCellAsync(string boxCode, string cellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    // 查找容器
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    // 检查容器是否已绑定库位
                    if (box.CellData.CellCode == null)
                        return new ResponseDto() { success = false, message = $"容器未绑定库位" };

                    // 查找库位
                    var cell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{box.CellData.CellCode}的库位不存在" };

                    // 解绑容器
                    box.DisBindCell();
                    await _boxRepository.UpdateAsync(box);

                    // 更新库位状态
                    cell.SetCellStatus(CellStatus.Nohave);
                    await _cellRepository.UpdateAsync(cell);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "容器解绑库位成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> CreatePipelineInAsync(string boxCode, decimal height, decimal weight, string plpeCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    string startCellCode = null;
                    switch (plpeCode)
                    {
                        case "1":
                            startCellCode = "700030B1501013";
                            break;
                        case "2":
                            startCellCode = "700020A9501013";
                            break;
                        default:
                            return new ResponseDto() { success = false, message = $"该输送线编号未设置" };
                    }

                    var cell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCellCode}的库位不存在" };

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                        return new ResponseDto() { success = false, message = $"料箱已存在任务" };

                    if (plpeCode == "1")
                    {
                        if(box.BoxTypeName!="2")
                            return new ResponseDto() { success = false, message = $"容器类型错误，不是托盘" };

                        //呼叫任务
                        Cell endCell = await _cellRepository.FirstOrDefaultAsync(t => t.WarehouseAreaId == 4 && t.CellType == CellType.Cell
                        && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave);

                        if(endCell == null)
                        {
                            _logger.Debug("分配库位失败");
                            return new ResponseDto() { success = false, message = $"分配库位失败" };
                        }


                        _logger.Debug("绑定托盘");

                        //RCS容器绑定库位
                        await _agvTaskManager.BindCtnrAndBinAsync(cell.CellCode, box.BoxTypeName, box.BoxCode, "1");

                        _logger.Debug(JsonConvert.SerializeObject(endCell));
                        await SetAsExecutingAsync(cell, endCell, null, box, ManageType.LiftSSXIn).ConfigureAwait(false);
                        await uow.SaveChangesAsync();
                    }
                    else
                    {
                        //ctu送入库区
                        if (box.BoxTypeName != "1")
                            return new ResponseDto() { success = false, message = $"容器类型错误，不是料箱" };

                        List<Cell> skipCells = await _cellRepository.FindByAreaTypeAvailableAsync(5,CellType.Skip,"1").ConfigureAwait(false);

                        var skips = await _skipRepository.FindInZhouZhuanAsync(skipCells.Select(o => o.Id).ToList(), 1);

                        _logger.Debug(JsonConvert.SerializeObject(skips));

                        if (skips.Where(t => t.SkipRunStatus != SkipRunStatus.OutByWare).Count() > 0)
                        {
                            List<Cell> cells = await _cellRepository.FindByZhouZhuanCellAsync(skips.Select(o => o.SkipCode).ToList());

                            Cell endCell = cells.Where(t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave).FirstOrDefault();

                            if (endCell == null)
                            {
                                return new ResponseDto() { success = false, message = $"没有可用入库点位" };
                            }

                            Console.WriteLine("预调度");
                            await _agvTaskManager.CreatePreAsync("101", "80", "11");

                            Console.WriteLine("下发任务");
                            await SetAsExecutingAsync(cell, endCell, endCell.ShelfName, box, ManageType.CTUSSXIn).ConfigureAwait(false);
                            await uow.SaveChangesAsync();

                            endCell.SetSelected();
                            await _cellRepository.UpdateAsync(endCell);
                            await uow.SaveChangesAsync();
                        }
                        else
                        {
                            return new ResponseDto() { success = false, message = $"没有可用料车点位" };
                        }
                    }

                    box.Height = height;
                    box.Weight = weight;
                    Console.WriteLine(JsonConvert.SerializeObject(box));
                    await _boxRepository.UpdateAsync(box);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "容器绑定库位成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        //容器入库
        [UnitOfWork]
        public async Task<ResponseDto> CreateCTUBasicInAsync(string boxCode, string cellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    var startCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                    if(startCell == null)
                        return new ResponseDto() { success = false, message = $"该容器未绑定库位" };

                    //分配库位
                    var endCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);

                    var skip = await _skipRepository.FindBySkipCodeAsync(startCell.ShelfName).ConfigureAwait(false);

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                        return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };


                    if (box.BoxTypeName == "1")
                    {
                        if (startCell.RunStatus != CellRunStatus.Enable || startCell.CellStatus != CellStatus.Have)
                            return new ResponseDto() { success = false, message = $"开始库位状态错误" };
                    }

                    if (endCell.RunStatus != CellRunStatus.Enable || endCell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"结束库位状态错误" };


                    if (startCell.CellType == CellType.SkipCell)
                    {
                        if (box.BoxTypeName == "1")
                        {
                            if (skip == null)
                                return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                            var skipCell = await _cellRepository.FindByIdAsync(skip.CellId.GetValueOrDefault()).ConfigureAwait(false);
                            if (skipCell == null)
                                return new ResponseDto() { success = false, message = $"读取料车库位失败，该料车未绑定库位" };

                            if (skipCell.WarehouseAreaId != 4)
                                return new ResponseDto() { success = false, message = $"起始料车不在周转区，无法入库" };

                            if (skip.Type != 1 && skip.Type != 2)
                                return new ResponseDto() { success = false, message = $"料车类型与托盘类型不匹配" };

                            if (endCell.CellType != CellType.CTUCell)
                                return new ResponseDto() { success = false, message = $"结束库位类型错误" };



                            _logger.Info("开始创建agv任务");

                            await SetAsExecutingAsync(startCell, endCell, skip.SkipCode, box, ManageType.CTUStockIn).ConfigureAwait(false);
                        }
                        else if (box.BoxTypeName == "2")
                        {
                            Cell startLiftCell = await _cellRepository.FindByCellCode2Async(box.CellData.CellCode);
                            skip = await _skipRepository.FindByCellIdAsync(startLiftCell.Id);
                            if (skip == null)
                                return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                            var skipCell = await _cellRepository.FindByIdAsync(skip.CellId.GetValueOrDefault()).ConfigureAwait(false);
                            if (skipCell == null)
                                return new ResponseDto() { success = false, message = $"读取料车库位失败，该料车未绑定库位" };

                            if (skipCell.WarehouseAreaId != 4)
                                return new ResponseDto() { success = false, message = $"起始料车不在周转区，无法入库" };
                            if (skip.Type != 3)
                                return new ResponseDto() { success = false, message = $"料车类型与托盘类型不匹配" };

                            if (endCell.CellType != CellType.Cell)
                                return new ResponseDto() { success = false, message = $"结束库位类型错误" };


                            _logger.Info("开始创建agv任务");

                            await SetAsExecutingAsync(startCell, endCell, skip.SkipCode, box, ManageType.LiftStockIn).ConfigureAwait(false);
                        }
                        else
                        {
                            return new ResponseDto() { success = false, message = $"容器类型错误" };
                        }
                    }
                    else if (startCell.CellType == CellType.Cell || startCell.CellType == CellType.Skip)
                    {
                        if(box.BoxTypeName != "2")
                            return new ResponseDto() { success = false, message = $"容器类型错误" };

                        if (endCell.CellType != CellType.Cell)
                            return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                        if(startCell.WarehouseAreaId!=4)
                            return new ResponseDto() { success = false, message = $"起始库位不在周转区，无法入库" };

                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.LiftStockIn).ConfigureAwait(false);
                    }
                    else if (startCell.CellType == CellType.WallCell)
                    {
                        if (box.BoxTypeName != "1")
                            return new ResponseDto() { success = false, message = $"容器类型错误" };

                        if (endCell.CellType != CellType.CTUCell)
                            return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.CTUStockIn).ConfigureAwait(false);
                    }
                    else
                    {
                        return new ResponseDto() { success = false, message = $"开始库位类型错误" };
                    }
                    await uow.SaveChangesAsync();


                    endCell.SetSelected();
                    await _cellRepository.UpdateAsync(endCell);
                    await uow.SaveChangesAsync();


                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建入库任务成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> CreateCTUSkipStockInAsync(SkipStockCtuInDto dtos)
        {
            try
            {

                var skip = await _skipRepository.FindBySkipCodeAsync(dtos.SkipCode).ConfigureAwait(false);

                if (skip == null)
                    return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                var skipCell = await _cellRepository.FindByIdAsync(skip.CellId.GetValueOrDefault()).ConfigureAwait(false);
                if (skipCell == null)
                    return new ResponseDto() { success = false, message = $"读取料车库位失败，该料车未绑定库位" };

                if (skipCell.WarehouseAreaId != 4)
                    return new ResponseDto() { success = false, message = $"起始料车不在周转区，无法入库" };

                if (skip.SkipRunStatus != SkipRunStatus.In)
                    return new ResponseDto() { success = false, message = $"起始料车不是入库料车，无法整车入库" };

                if (skip.Type != 1 && skip.Type != 2)
                    return new ResponseDto() { success = false, message = $"料车类型与托盘类型不匹配" };

                if(dtos.SkipStocksCtuIn.Count==0)
                    return new ResponseDto() { success = false, message = $"没有入库数据，请稍等" };

                foreach (StocksCtuInDto dto in dtos.SkipStocksCtuIn)
                {

                    using (var uow = UnitOfWorkManager.Begin(true, true))
                    {
                        var box = await _boxRepository.FindByBoxCodeAsync(dto.BoxCode).ConfigureAwait(false);
                        if (box == null)
                            return new ResponseDto() { success = false, message = $"容器码为{dto.BoxCode}的容器不存在" };

                        var startCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                        if (startCell == null)
                            return new ResponseDto() { success = false, message = $"该容器未绑定库位" };

                        var endCell = await _cellRepository.FindByCellCodeAsync(dto.EndCode).ConfigureAwait(false);
                        if (endCell == null)
                            return new ResponseDto() { success = false, message = $"库位码为{dto.EndCode}的库位不存在" };

                        if (await _agvTaskManager.IsExistBoxTask(dto.BoxCode))
                            return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };

                        if (box.BoxTypeName != "1")
                            return new ResponseDto() { success = false, message = $"该容器不是CTU料箱" };


                        if (startCell.RunStatus != CellRunStatus.Enable || startCell.CellStatus != CellStatus.Have)
                            return new ResponseDto() { success = false, message = $"开始库位状态错误" };

                        if (endCell.RunStatus != CellRunStatus.Enable || endCell.CellStatus != CellStatus.Nohave)
                            return new ResponseDto() { success = false, message = $"结束库位状态错误" };

                        if (startCell.CellType != CellType.SkipCell)
                            return new ResponseDto() { success = false, message = $"开始库位类型错误" };

                        if (endCell.CellType != CellType.CTUCell)
                            return new ResponseDto() { success = false, message = $"结束库位类型错误" };



                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, skip.SkipCode, box, ManageType.CTUStockIn).ConfigureAwait(false);
                        await uow.SaveChangesAsync();

                        endCell.SetSelected();
                        await _cellRepository.UpdateAsync(endCell);
                        await uow.SaveChangesAsync();

                        await uow.CompleteAsync().ConfigureAwait(false);
                    }
                }

                skip.SkipStatus = SkipStatus.NoHave;
                skip.SkipRunStatus = SkipRunStatus.Enable;
                skip.TargetLocation = null;
                await _skipRepository.UpdateAsync(skip);

                return new ResponseDto() { success = true, message = "创建入库任务成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> CreateCTUCheckInAsync(string barcode, string boxCode, string startCellCode, string endCellCode, decimal count)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    var startCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                    if (startCell == null)
                        return new ResponseDto() { success = false, message = $"该容器未绑定库位" };

                    var endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                    if (endCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{boxCode}的库位不存在" };

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                        return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };

                    //if (box.BoxTypeName != "1")
                    //    return new ResponseDto() { success = false, message = $"该容器不是CTU容器" };

                    if (startCell.RunStatus != CellRunStatus.Enable && startCell.CellStatus != CellStatus.Have)
                        return new ResponseDto() { success = false, message = $"开始库位状态错误" };
                    if (startCell.CellType != CellType.WallCell && startCell.CellType != CellType.Cell)
                        return new ResponseDto() { success = false, message = $"开始库位类型错误" };

                    if (endCell.RunStatus != CellRunStatus.Enable && endCell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"结束库位状态错误" };
                    if (endCell.CellType != CellType.CTUCell && endCell.CellType!=CellType.Cell)
                        return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(startCell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)startCell.WarehouseAreaId);


                    var stocks=await _stockRepository.GetByBoxIdAsync(box.Id);
                    if (!stocks.Select(b=>b.Barcode).Contains(barcode))
                        return new ResponseDto() { success = false, message = $"该容器不存在该物料，无法检验/复检入库" };

                    List<ChkResultList> chkList = await _chkResultListRepository.FindByBarcodeAsync(barcode);
                    ChkResultList chk = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);
                    if (chk == null)
                    {
                        BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(barcode).ConfigureAwait(false);
                        if (barcodeResult == null)
                            throw new Exception($"收料码为{barcode}的收料条目不存在");
                        if (barcodeResult.Status == ChkResultListStatus.Finished)
                            throw new Exception($"收料码{barcode}已全部完成绑定");

                        if (count > barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBindCount)
                            throw new Exception($"绑定数量大于本收料条码剩余未绑定数量,剩余数量为" + (barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBindCount));

                        barcodeResult.InBindCount += count;
                        await _barcodeListRepository.UpdateAsync(barcodeResult);
                    }
                    else
                    {
                        if(count > chk.CheckData.PassCnt-chk.InBoundedCount)
                            throw new Exception($"复检单的物料剩余可入库{chk.CheckData.PassCnt - chk.InBoundedCount}，实际却准备入库{count}");
                        chk.InBoundedCount += count;
                        if (chk.InBoundedCount == chk.CheckData.PassCnt)
                        {
                            chk.setStatus(ChkResultListStatus.Finished);
                        }
                        else
                        {
                            chk.setStatus(ChkResultListStatus.Used);
                        }
                        await _chkResultListRepository.UpdateAsync(chk);
                    }

                    var stock = stocks.Where(t => t.Barcode == barcode).FirstOrDefault();


                    //检查是否满箱
                    decimal fullRate = await BoxIsFul(box.Id, stock.Material.MaterialCode, count);
                    if (fullRate > 1)
                    {
                        return new ResponseDto() { success = false, message = $"料箱已满箱" };
                    }
                    stock.CombineStock(count);


                    await _stockRepository.UpdateAsync(stock).ConfigureAwait(false);

                    _logger.Info("开始创建agv任务");
                    if (box.BoxTypeName == "1")
                    {
                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.CTUStockIn).ConfigureAwait(false);
                    }
                    else
                    {
                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.LiftStockIn).ConfigureAwait(false);
                    }
                    await uow.SaveChangesAsync();


                    endCell.SetSelected();
                    await _cellRepository.UpdateAsync(endCell);



                    await uow.SaveChangesAsync();
                    await _stocksManager.BoxFullRate(box.Id);
                    await uow.SaveChangesAsync();

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建入库任务成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> CreateLiftInAsync(List<StockCreateDto> paras, string boxCode, string startCellCode, string endCellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的托盘不存在" };

                    if (await _agvTaskManager.IsExistBoxTask(boxCode))
                        return new ResponseDto() { success = false, message = $"该托盘已存在AGV任务" };

                    if (box.BoxTypeName != "2")
                        return new ResponseDto() { success = false, message = $"该容器不是托盘" };

                    var startCell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (startCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCell}的库位不存在" };

                    var endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                    if (endCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{endCellCode}的库位不存在" };

                    if (startCell.RunStatus != CellRunStatus.Enable)
                        return new ResponseDto() { success = false, message = $"开始库位状态错误" };

                    if (startCell.CellType != CellType.Cell)
                        return new ResponseDto() { success = false, message = $"开始库位类型错误" };

                    if (startCell.WarehouseAreaId != 4)
                        return new ResponseDto() { success = false, message = $"入库库位不是周转区库位，不允许入库" };

                    if (endCell.RunStatus != CellRunStatus.Enable || endCell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"结束库位状态错误" };

                    if (endCell.CellType != CellType.Cell)
                        return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                    List<BarcodeCheck> checkList = await _barcodeCheckRepository.GetByBoxAsync(box.Id);

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(startCell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)startCell.WarehouseAreaId);

                    //检查是否满箱
                    decimal fullRate = await BoxIsFul(box.Id, paras);
                    if (fullRate > 1)
                    {
                        return new ResponseDto() { success = false, message = $"料箱已满箱" };
                    }

                    //检查是否有同类型物料
                    if (!await BoxIsSameTypeCTU(box.Id, paras))
                    {
                        return new ResponseDto() { success = false, message = $"料箱已有同类型物料或组盘中有同类型物料" };
                    }

                    foreach (var para in paras)
                    {
                        BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(para.Barcode).ConfigureAwait(false);
                        if (barcodeResult == null)
                        {
                            await uow.RollbackAsync();
                            return new ResponseDto() { success = false, message = $"收料码为{para.Barcode}的收料条目不存在" };
                        }
                        if (barcodeResult.Status == ChkResultListStatus.Finished)
                        {
                            await uow.RollbackAsync();
                            return new ResponseDto() { success = false, message = $"收料码{para.Barcode}已完成入库" };
                        }

                        if (para.TotalCount > barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBindCount)
                        {
                            await uow.RollbackAsync();
                            return new ResponseDto() { success = false, message = $"绑定数量大于本收料条码剩余未绑定数量,剩余数量为" + (barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBindCount) };
                        }

                        

                        if (barcodeResult.IsTag == 1)
                        {
                            if (checkList.Where(t => t.BarcodeId == barcodeResult.Id).Count() == 0)
                            {
                                BarcodeCheck checks = await _barcodeCheckRepository.GetByBarcodeAsync(barcodeResult.Id);
                                if (checks == null)
                                {
                                    await uow.RollbackAsync();
                                    return new ResponseDto() { success = false, message = $"收料码{para.Barcode}未抽检，无法入库" };
                                }
                            }
                        }


                        MaterialInfoOfStock material = new MaterialInfoOfStock(
                                barcodeResult.Material.MaterialCode,
                                barcodeResult.Material.MaterialName,
                                barcodeResult.Material.Specs,
                                barcodeResult.Material.Unit,
                                null);

                        CountInfoOfStock countInfo = new CountInfoOfStock(
                                barcodeResult.ReceiveCount.ReceiveTotalCount,
                                barcodeResult.ReceiveCount.ReceivePkgOrBoxCount,
                                barcodeResult.ReceiveCount.CountInOnePkgOrBox);

                        SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                                barcodeResult.Supplier.SupplierCode,
                                barcodeResult.Supplier.SupplierName,
                                barcodeResult.Supplier.SupplierBatchCode);


                        var stock = await _stocksManager.CreateStockAsync(
                            para.Barcode,
                            para.TotalCount,
                            material,
                            countInfo,
                            supplierInfo,
                            barcodeResult.StockInType,
                            barcodeResult.IsTag,
                            barcodeResult.BatchCode,
                            barcodeResult.BLCode,
                            barcodeResult.BHCode);

                        var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, para.Barcode).ConfigureAwait(false);
                        if (stockExist != null)
                        {
                            stockExist.CombineStock(stock);
                            await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                        }
                        else
                        {
                            stock.BindBox(box.Id, box.BoxCode, box.BoxName);
                            await _stockRepository.InsertAsync(stock).ConfigureAwait(false);

                            var stockInHistory = new StockInHistory(
                                stock.Barcode,
                                barcodeResult.Material.MaterialCode,
                                barcodeResult.Material.MaterialName,
                                barcodeResult.Material.Specs,
                                barcodeResult.Material.Unit,
                                warehouse.WarehouseCode,
                                warehouse.WarehouseName,
                                warehouseArea.WarehouseAreaCode,
                                warehouseArea.WarehouseAreaName,
                                startCell.CellCode,
                                startCell.CellName,
                                box.BoxCode,
                                box.BoxName,
                                StockInTypeHelper.StockInTypeToChinese(barcodeResult.StockInType),
                                para.TotalCount,
                                DateTime.Now,
                                batchNo: barcodeResult.BatchCode);

                            await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);
                        }

                        barcodeResult.InBindCount += para.TotalCount;
                        await _barcodeListRepository.UpdateAsync(barcodeResult);
                    }

                    box.BindCell(startCell, warehouse, warehouseArea);
                    await _boxRepository.UpdateAsync(box);


                    await _agvTaskManager.BindCtnrAndBinAsync(startCellCode, box.BoxTypeName, box.BoxCode, "1");


                    _logger.Info("开始创建agv任务");
                    await SetAsExecutingAsync(startCell, endCell, "", box, ManageType.LiftStockIn).ConfigureAwait(false);
                    await uow.SaveChangesAsync();


                    endCell.SetSelected();
                    await _cellRepository.UpdateAsync(endCell);

                    await uow.SaveChangesAsync();
                    await _stocksManager.BoxFullRate(box.Id);
                    await uow.SaveChangesAsync();

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建托盘入库任务成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> CreateStockAndBindCellAsync(string barcode, string cellCode, string boxCode,decimal pkgCount, decimal partsCount)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位不存在" };

                    var house = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    if (house == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的仓库Id有误" };

                    if (cell.WarehouseAreaId == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位没有包含库区信息" };

                    var area = house.GetAreaByAreaId(cell.WarehouseAreaId.Value);
                    if (area == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的库区Id有误" };

                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode, false).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(barcode).ConfigureAwait(false);
                    if (barcodeResult == null)
                        throw new Exception($"收料码为{barcode}的收料条目不存在");
                    if (barcodeResult.Status == ChkResultListStatus.Finished)
                        throw new Exception($"收料码{barcode}已完成入库");

                    decimal count = pkgCount * barcodeResult.ReceiveCount.CountInOnePkgOrBox.GetValueOrDefault() + partsCount;
                    if (count > barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBoundedCount)
                        throw new Exception($"入库数量大于本收料条码剩余未入库数量,剩余数量为" + (barcodeResult.ReceiveCount.ReceiveTotalCount - barcodeResult.InBoundedCount));



                    MaterialInfoOfStock material = new MaterialInfoOfStock(
                            barcodeResult.Material.MaterialCode,
                            barcodeResult.Material.MaterialName,
                            barcodeResult.Material.Specs,
                            barcodeResult.Material.Unit,null);

                    CountInfoOfStock countInfo = new CountInfoOfStock(
                            barcodeResult.ReceiveCount.ReceiveTotalCount,
                            barcodeResult.ReceiveCount.ReceivePkgOrBoxCount,
                            barcodeResult.ReceiveCount.CountInOnePkgOrBox);

                    SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                            barcodeResult.Supplier.SupplierCode,
                            barcodeResult.Supplier.SupplierName,
                            barcodeResult.Supplier.SupplierBatchCode);


                    var stock = await _stocksManager.CreateStockAsync(
                        barcode,
                        count,
                        material,
                        countInfo,
                        supplierInfo,
                        barcodeResult.StockInType,
                        barcodeResult.IsTag,
                        barcodeResult.BatchCode,
                        barcodeResult.BLCode,
                        barcodeResult.BHCode);

                    var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode).ConfigureAwait(false);
                    if (stockExist != null)
                    {
                        //stockExist.CombineStock(stock);
                        await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                    }
                    else
                    {
                        //stock.BindBox(box.Id, box.BoxCode, box.BoxName);
                        stock.BindCell(cell.Id, cell.CellCode, cell.CellName,cell.AvailableBoxSpecsNames,cell.CellType,
                            area.Id, area.WarehouseAreaCode, area.WarehouseAreaName,
                            house.Id, house.WarehouseCode, house.WarehouseName);
                        await _stockRepository.InsertAsync(stock).ConfigureAwait(false);

                        var stockInHistory = new StockInHistory(
                            stock.Barcode,
                            barcodeResult.Material.MaterialCode,
                            barcodeResult.Material.MaterialName,
                            barcodeResult.Material.Specs,
                            barcodeResult.Material.Unit,
                            house.WarehouseCode,
                            house.WarehouseName,
                            area.WarehouseAreaCode,
                            area.WarehouseAreaName,
                            cell.CellCode,
                            cell.CellName,
                            box.BoxCode,
                            box.BoxName,
                            StockInTypeHelper.StockInTypeToChinese(barcodeResult.StockInType),
                            count,
                            DateTime.Now,
                            batchNo: barcodeResult.BatchCode);

                        await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);
                    }

                    barcodeResult.InBindCount += count;
                    barcodeResult.InBoundedCount += count;
                    await _barcodeListRepository.UpdateAsync(barcodeResult);

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);

                    box.BindCell(cell, warehouse, warehouseArea);

                    cell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(cell);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建库存并入库成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<bool> CheckReceivingMaterialBarcodeExistsAsync(string receivingMaterialBarcode)
        {
            if (string.IsNullOrWhiteSpace(receivingMaterialBarcode))
                return false;

            var existingStock = await _stockRepository.FindByReceivingMaterialBarcodeAsync(receivingMaterialBarcode).ConfigureAwait(false);
            return existingStock != null;
        }

        public async Task<ResponseDto> CreateStockAndBindCellNormalAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    if (paras == null || paras.Count == 0)
                        throw new ArgumentException("未指定待创建的库存");

                    var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位不存在" };

                    var house = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    if (house == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的仓库Id有误" };

                    if (cell.WarehouseAreaId == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位没有包含库区信息" };

                    var area = house.GetAreaByAreaId(cell.WarehouseAreaId.Value);
                    if (area == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的库区Id有误" };

                    var box = await _boxRepository.FindByBoxCodeAsync(cellCode, false).ConfigureAwait(false); //一期没有托盘，使用虚拟托盘，虚拟托盘号为库位号
                    if (box == null) //没有这个虚拟托盘
                        throw new Exception($"库位{cellCode}未绑定容器号为{cellCode}的虚拟容器");


                    foreach (var para in paras)
                    {
                        //东方电子一期的库存信息从检验结论中间表中获取
                        List<ChkResultList> chkResults = await _chkResultListRepository.FindByBarcodeAsync(para.Barcode).ConfigureAwait(false);
                        if (chkResults == null || chkResults.Count == 0)
                        {
                            var stocksExist = await _stockRepository.GetByBarcodeAsync(para.Barcode).ConfigureAwait(false);
                            if (stocksExist == null || stocksExist.Count == 0)
                                throw new Exception($"收料码为{para.Barcode}的检验结论不存在");
                            else
                                throw new Exception($"收料码为{para.Barcode}的检验结论不存在，但存在收料码为{para.Barcode}的库存，请检查是否已入库");
                        }

                        if (chkResults.Count > 1)
                            throw new Exception($"同时出现多个收料码为{para.Barcode}的检验结论");

                        ChkResultList chkResult = chkResults[0];

                        MaterialInfoOfStock material = new MaterialInfoOfStock(
                                chkResult.Material.MaterialCode,
                                chkResult.Material.MaterialName,
                                chkResult.Material.Specs,
                                chkResult.Material.Unit,null);

                        CountInfoOfStock countInfo = new CountInfoOfStock(
                                chkResult.ReceiveCount.ReceiveTotalCount,
                                chkResult.ReceiveCount.ReceivePkgOrBoxCount,
                                chkResult.ReceiveCount.CountInOnePkgOrBox);

                        CheckInfoOfStock checkInfo = new CheckInfoOfStock(
                                chkResult.CheckData.CheckOrderCode,
                                chkResult.CheckData.CheckDate,
                                chkResult.CheckData.CheckNo,
                                chkResult.CheckData.CheckNoBeforeReCheck,
                                chkResult.CheckData.CheckType,
                                chkResult.CheckData.CheckResult,
                                chkResult.CheckData.PassCnt);

                        SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                                chkResult.Supplier.SupplierCode,
                                chkResult.Supplier.SupplierName,
                                "");


                        var stock = await _stocksManager.CreateStockAsync(
                            para.Barcode,
                            para.TotalCount,
                            material,
                            countInfo,
                            supplierInfo,
                            //checkInfo,
                            chkResult.StockInType,
                            1,
                            chkResult.BatchCode,
                            chkResult.BLCode,
                            chkResult.BHCode);

                        //通知检验结论对象更新数据
                        await _localEventBus.PublishAsync(new StockBindBoxAndCellEvent()
                        {
                            StockBarcode = chkResult.Barcode,
                            CheckType = chkResult.CheckData.CheckType,
                            BoxId = box.Id,
                            BoxCode = box.BoxCode,
                            BoxName = box.BoxName,
                            CellId = cell.Id,
                            CellCode = cell.CellCode,
                            CellName = cell.CellName,
                            AreaId = area.Id,
                            AreaCode = area.WarehouseAreaCode,
                            AreaName = area.WarehouseAreaName,
                            HouseId = house.Id,
                            HouseCode = house.WarehouseCode,
                            HouseName = house.WarehouseName,
                            StockCount = para.TotalCount,

                            MaterialCode = material.MaterialCode,
                            MaterialName = material.MaterialName,
                            Specs = material.Specs,
                            Unit = material.Unit,

                            CheckOrderCode = checkInfo.CheckOrderCode,
                            CheckDate = checkInfo.CheckDate,
                            CheckNo = checkInfo.CheckNo,
                            CheckNoBeforeReCheck = checkInfo.CheckNoBeforeReCheck,
                            CheckResult = checkInfo.CheckResultInChs(),
                            PassCnt = checkInfo.PassCnt,

                            SupplierCode = supplierInfo.SupplierCode,
                            SupplierName = supplierInfo.SupplierName,

                            StockInDate = stock.StockInDate,
                            StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                            BatchCode = stock.BatchCode,
                            BLCode = stock.BLCode,
                            BHCode = stock.BHCode,
                            CheckTypeInChs = checkInfo.CheckTypeInChs(),
                            Operator = operatorName
                        });

                        if (stock.StockInType == StockInType.RecheckStockIn)
                            throw new Exception("当前入库类型为超期复检入库，请到超期复检入库界面执行");

                        var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, para.Barcode).ConfigureAwait(false);
                        if (stockExist != null)
                        {
                            //stockExist.CombineStock(stock);
                            await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                        }
                        else
                        {
                            //stock.BindBox(box.Id, box.BoxCode, box.BoxName);
                            stock.BindCell(cell.Id, cell.CellCode, cell.CellName,cell.AvailableBoxSpecsNames,cell.CellType,
                                area.Id, area.WarehouseAreaCode, area.WarehouseAreaName,
                                house.Id, house.WarehouseCode, house.WarehouseName);

                            var existingStock = await _stockRepository.FindByReceivingMaterialBarcodeAsync(chkResult.Barcode).ConfigureAwait(false);
                            if (existingStock != null)
                            {
                                throw new Exception($"收料码为{chkResult.Barcode}的库存已存在，不能重复创建");
                            }

                            await _stockRepository.InsertAsync(stock).ConfigureAwait(false);

                            var stockInHistory = new StockInHistory(
                                stock.Barcode,
                                chkResult.Material.MaterialCode,
                                chkResult.Material.MaterialName,
                                chkResult.Material.Specs,
                                chkResult.Material.Unit,
                                house.WarehouseCode,
                                house.WarehouseName,
                                area.WarehouseAreaCode,
                                area.WarehouseAreaName,
                                cell.CellCode,
                                cell.CellName,
                                box.BoxCode,
                                box.BoxName,
                                StockInTypeHelper.StockInTypeToChinese(chkResult.StockInType),
                                para.TotalCount,
                                DateTime.Now,
                                operatorName: operatorName,
                                batchNo: chkResult.BatchCode);

                            await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);
                        }
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建库存并入库成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> CreateStockAndInBoundAfterReCheckAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    if (paras == null || paras.Count == 0)
                        throw new ArgumentException("未指定待创建的库存");

                    var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位不存在" };

                    var house = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    if (house == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的仓库Id有误" };

                    if (cell.WarehouseAreaId == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位没有包含库区信息" };

                    var area = house.GetAreaByAreaId(cell.WarehouseAreaId.Value);
                    if (area == null)
                        return new ResponseDto() { success = false, message = $"库位码为{cellCode}的库位携带的库区Id有误" };

                    var box = await _boxRepository.FindByBoxCodeAsync(cellCode, false).ConfigureAwait(false); //一期没有托盘，使用虚拟托盘，虚拟托盘号为库位号
                    if (box == null) //没有这个虚拟托盘
                        throw new Exception($"库位{cellCode}未绑定容器号为{cellCode}的虚拟容器");

                    /*
                    foreach (var para in paras)
                    {
                        //东方电子一期的库存信息从检验结论中间表中获取
                        List<ChkResultList> chkResults = await _chkResultListRepository.FindByBarcodeAsync(para.Barcode).ConfigureAwait(false);
                        if (chkResults == null || chkResults.Count == 0)
                            throw new Exception($"收料码为{para.Barcode}的检验结论不存在");

                        if (chkResults.Count > 1)
                            throw new Exception($"同时出现多个收料码为{para.Barcode}的检验结论");

                        ChkResultList chkResult = chkResults[0];

                        MaterialInfoOfStock material = new MaterialInfoOfStock(
                                chkResult.Material.MaterialCode,
                                chkResult.Material.MaterialName,
                                chkResult.Material.Specs,
                                chkResult.Material.Unit);

                        CountInfoOfStock countInfo = new CountInfoOfStock(
                                chkResult.ReceiveCount.ReceiveTotalCount,
                                chkResult.ReceiveCount.ReceivePkgOrBoxCount,
                                chkResult.ReceiveCount.CountInOnePkgOrBox);

                        CheckInfoOfStock checkInfo = new CheckInfoOfStock(
                                chkResult.CheckData.CheckOrderCode,
                                chkResult.CheckData.CheckDate,
                                chkResult.CheckData.CheckNo,
                                chkResult.CheckData.CheckNoBeforeReCheck,
                                chkResult.CheckData.CheckType,
                                chkResult.CheckData.CheckResult,
                                chkResult.CheckData.PassCnt);

                        SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                                chkResult.Supplier.SupplierCode,
                                chkResult.Supplier.SupplierName);

                        //var stock = await _stocksManager.CreateStockAsync(
                        //    para.Barcode,
                        //    para.TotalCount,
                        //    material,
                        //    countInfo,
                        //    checkInfo,
                        //    supplierInfo,
                        //    chkResult.StockInType,
                        //    chkResult.BatchCode,
                        //    chkResult.BLCode,
                        //    chkResult.BHCode);

                        /*
                        //通知检验结论对象更新数据
                        await _localEventBus.PublishAsync(new StockBindBoxAndCellEvent()
                        {
                            StockBarcode = chkResult.Barcode,
                            CheckType = chkResult.CheckData.CheckType,
                            BoxId = box.Id,
                            BoxCode = box.BoxCode,
                            BoxName = box.BoxName,
                            CellId = cell.Id,
                            CellCode = cell.CellCode,
                            CellName = cell.CellName,
                            AreaId = area.Id,
                            AreaCode = area.WarehouseAreaCode,
                            AreaName = area.WarehouseAreaName,
                            HouseId = house.Id,
                            HouseCode = house.WarehouseCode,
                            HouseName = house.WarehouseName,
                            StockCount = para.TotalCount,

                            MaterialCode = material.MaterialCode,
                            MaterialName = material.MaterialName,
                            Specs = material.Specs,
                            Unit = material.Unit,

                            CheckOrderCode = checkInfo.CheckOrderCode,
                            CheckDate = checkInfo.CheckDate,
                            CheckNo = checkInfo.CheckNo,
                            CheckNoBeforeReCheck = checkInfo.CheckNoBeforeReCheck,
                            CheckResult = checkInfo.CheckResultInChs(),
                            PassCnt = checkInfo.PassCnt,

                            SupplierCode = supplierInfo.SupplierCode,
                            SupplierName = supplierInfo.SupplierName,

                            StockInDate = stock.StockInDate,
                            StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                            BatchCode = stock.BatchCode,
                            BLCode = stock.BLCode,
                            BHCode = stock.BHCode,
                            CheckTypeInChs = checkInfo.CheckTypeInChs(),
                            Operator = operatorName
                        });

                        if (stock.StockInType != StockInType.RecheckStockIn)
                            throw new Exception("当前入库类型非超期复检入库，请到常规入库界面执行");

                        List<Stock> stocksExist = await _stockRepository.GetByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                        if (stocksExist != null && stocksExist.Count > 0) //库存中有相同的收料条形码，需进入相同的库位
                        {
                            var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, stock.Barcode).ConfigureAwait(false);
                            if (stockExist == null)
                                throw new Exception($"收料码为{stock.Barcode}的库存属于超期复检入库，需要入到收料码相同的库位，但当前入库的库位没有该收料码");

                            stockExist.CombineStock(stock);
                            await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                        }
                        else  //库存中没有相同的收料条形码，可以进入其它库位
                        {
                            stock.BindBox(box.Id, box.BoxCode, box.BoxName);
                            stock.BindCell(cell.Id, cell.CellCode, cell.CellName,
                                area.Id, area.WarehouseAreaCode, area.WarehouseAreaName,
                                house.Id, house.WarehouseCode, house.WarehouseName);

                            await _stockRepository.InsertAsync(stock).ConfigureAwait(false);
                        }
                    }
                    */

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建库存并入库成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<List<CellWithMaterialDto>> GetCellsWithMaterialAsync(string materialCode,string uniqueCode)
        {
            try
            {
                var stocks = await _stockRepository.GetByMaterialCodeAsync(materialCode).ConfigureAwait(false);
                if (stocks == null || stocks.Count == 0)
                    return new List<CellWithMaterialDto>();

                if (uniqueCode != null)
                {
                    PickItem item = await _pickItemRepository.GetByUnique(uniqueCode);
                    if (item != null && !(item.CheckNo.IsNullOrWhiteSpace()))
                    {
                        stocks = stocks.Where(t => (t.CheckData.CheckNo == item.CheckNo)).OrderBy(t => t.CheckData.CheckDate).ToList();
                    }
                    else
                    {
                        stocks = stocks.Where(t => (t.Warehouse.AreaId == 1)
                            && t.Status == StockStatus.Available).OrderBy(t => t.CheckData.CheckDate).ToList();
                    }
                }
                else
                {
                    stocks = stocks.Where(t => (t.Warehouse.AreaId == 1)
                        && t.Status == StockStatus.Available).OrderBy(t => t.CheckData.CheckDate).ToList();
                }

                List<CellWithMaterialDto> result = new List<CellWithMaterialDto>();
                foreach (var stock in stocks)
                {
                    if (stock.CellData.CellId == null || stock.CellData.CellCode == null || stock.CellData.CellName == null)
                        continue;

                    Cell boxcell = await _cellRepository.FindByCellCodeAsync(stock.CellData.CellCode);

                    if(boxcell.RunStatus != CellRunStatus.Enable)
                    {
                        continue;
                    }

                    //在仓库中
                    //if (stock.Warehouse.AreaId != 1 || stock.Warehouse.AreaId != 2 || stock.Warehouse.AreaId != 3)
                    //    continue;

                    CellWithMaterialDto cell = new CellWithMaterialDto()
                    {
                        CellId = stock.CellData.CellId.Value,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        StockCount = stock.TotalCountInTime,
                        CheckNo = stock.CheckData.CheckNo,
                        BoxCode = stock.BoxData.BoxCode,
                        Barcode = stock.Barcode
                    };
                    result.Add(cell);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<CellWithMaterialDto>> GetCellsWithBarcodeAsync(string barcode)
        {
            try
            {
                var stocks = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
                if (stocks == null || stocks.Count == 0)
                    return new List<CellWithMaterialDto>();

                //stocks = stocks.Where(t => (t.Warehouse.AreaId == 1)
                //    && t.Status == StockStatus.Available).ToList();

                List<CellWithMaterialDto> result = new List<CellWithMaterialDto>();
                foreach (var stock in stocks)
                {
                    if (stock.CellData.CellId == null || stock.CellData.CellCode == null || stock.CellData.CellName == null)
                        continue;

                    //在仓库中
                    if (stock.Warehouse.AreaId != 1 && stock.Warehouse.AreaId != 2 && stock.Warehouse.AreaId != 3)
                        continue;

                    CellWithMaterialDto cell = new CellWithMaterialDto()
                    {
                        CellId = stock.CellData.CellId.Value,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        StockCount = stock.TotalCountInTime,
                        CheckNo = stock.CheckData.CheckNo,
                        BoxCode = stock.BoxData.BoxCode
                    };
                    result.Add(cell);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<StockDto>> GetPagedStocksAsync(PagedStockQueryDto para)
        {
            try
            {
                //List<Stock> stocks1 = await _stockRepository.ToListAsync();
                //foreach (Stock stock1 in stocks1)
                //{
                //    if (stock1.CellData == null || stock1.CellData.CellCode == null)
                //        continue;
                //    Cell cell = await _cellRepository.FindByCellCodeAsync(stock1.CellData.CellCode);
                //    stock1.CellData.CellType = cell.CellType;
                //    await _stockRepository.UpdateAsync(stock1);
                //}


                int cellType = 0;
                switch (para.wareType)
                {
                    case 1://料箱
                        cellType = 2;
                        break;
                    case 2://托盘
                        cellType = 1;
                        break;
                    case 3://分拨墙
                        cellType = 3;
                        break;
                    case 4://手工
                        cellType = 10;
                        break;
                    default:
                        cellType = 0;
                        break;
                }

                var stocks = await _stockRepository.GetPagedStocksAsync(
                    para.BoxCode, para.CellCode, para.WarehouseAreaName, para.WarehouseName,
                    para.MaterialCode, para.MaterialNameTip, para.MaterialSpecsTip, para.Barcode,
                    para.ReceivingMaterialBarcode,
                    para.Status, para.StockInType, para.StockInDateStart, para.StockInDateEnd,
                    para.FullBoxRateStart,para.FullBOxRateEnd, cellType,para.FinGoods,
                    para.CheckType, para.CheckResult, para.CheckNo,
                    false,
                    para.SkipCount, para.MaxResultCount);


                List<StockDto> items = new List<StockDto>();

                PagedResultDto<StockDto> result = new PagedResultDto<StockDto>()
                {
                    TotalCount = stocks.TotalCount
                };

                Hashtable table = new Hashtable();

                foreach (var stock in stocks.Items)
                {
                    StockDto item = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        ReceivingMaterialBarcode = stock.ReceivingMaterialBarcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName,
                        FullBoxRate = stock.BoxData.FullRate,
                        AvaType = stock.CellData.AvaBoxType
                    };
                    items.Add(item);
                }
                result.Items = items;
                return result;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<List<StockDto>> GetStocksAsync(PagedStockQueryDto para)
        {
            try
            {
                int cellType = 0;
                switch (para.wareType)
                {
                    case 1://料箱
                        cellType = 2;
                        break;
                    case 2://托盘
                        cellType = 1;
                        break;
                    case 3://分拨墙
                        cellType = 3;
                        break;
                    case 4://手工
                        cellType = 10;
                        break;
                    default:
                        cellType = 0;
                        break;
                }

                var stocks = await _stockRepository.GetStocksAsync(
                    para.BoxCode, para.CellCode, para.WarehouseAreaName, para.WarehouseName,
                    para.MaterialCode, para.MaterialNameTip, para.MaterialSpecsTip, para.Barcode,
                    para.ReceivingMaterialBarcode,
                    para.Status, para.StockInType, para.StockInDateStart, para.StockInDateEnd,
                    para.FullBoxRateStart, para.FullBOxRateEnd, cellType, para.FinGoods,
                    para.CheckType, para.CheckResult, para.CheckNo,
                    false,
                    para.SkipCount, para.MaxResultCount);

                List<StockDto> stockDtos = new List<StockDto>();
                foreach (var stock in stocks)
                {
                    StockDto dto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        RunStatus = stock.RunStatus.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BoxData_BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName,
                        FullBoxRate = stock.BoxData.FullRate,
                        InspectionCount = stock.InspectionCount,
                        InspectionStatus = (int)(stock.InspectionStatus ?? 0)
                    };
                    stockDtos.Add(dto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<StockDto>> GetPagedStocksByMoveAsync(PagedStockMoveQueryDto para)
        {
            try
            {
                var stocks = await _stockRepository.GetPagedMoveStocksAsync(
                    para.AreaId, para.MaterialName, para.CheckNo,
                    false,
                    para.SkipCount, para.MaxResultCount);

                PagedResultDto<StockDto> result = new PagedResultDto<StockDto>()
                {
                    TotalCount = stocks.TotalCount
                };

                List<StockDto> items = new List<StockDto>();
                foreach (var stock in stocks.Items)
                {
                    StockDto item = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        ReceivingMaterialBarcode = stock.ReceivingMaterialBarcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    items.Add(item);
                }

                result.Items = items;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<StockDto>> GetCtuInStocksAsync(PagedStockQueryDto para)
        {
            try
            {
                var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();

                var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o=>o.Id).ToList(),1);

                var cells = await _cellRepository.FindByZhouZhuanCellAsync(skips.Select(o => o.SkipCode).ToList());

                var stocks = await _stockRepository.GetCtuInStocksAsync(
                    cells.Select(o => o.CellCode).ToList(),
                    para.SkipCount, para.MaxResultCount);

                PagedResultDto<StockDto> result = new PagedResultDto<StockDto>()
                {
                    TotalCount = stocks.TotalCount
                };

                List<StockDto> items = new List<StockDto>();
                foreach (var stock in stocks.Items)
                {
                    StockDto item = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        ReceivingMaterialBarcode = stock.ReceivingMaterialBarcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    items.Add(item);
                }

                var boxs = await _boxRepository.GetByCellsIdAsync(cells.Select(t=>t.Id).ToList());
                boxs = boxs.Where(t => t.Status == BoxStatus.NoHave).ToList();
                foreach(var box in boxs)
                {
                    StockDto item = new StockDto()
                    {
                        BoxId = box.Id,
                        BoxCode = box.BoxCode
                    };
                    items.Add(item);
                }

                result.Items = items;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<SkipInDto>> GetCtuInSkipAsync()
        {
            try
            {
                var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();

                var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o => o.Id).ToList(),1);

                skips = skips.Where(t => t.SkipRunStatus == SkipRunStatus.In).ToList();

                PagedResultDto<SkipInDto> result = new PagedResultDto<SkipInDto>();

                List<SkipInDto> items = new List<SkipInDto>();
                foreach (var skip in skips)
                {
                    List<Cell> cells = await _cellRepository.FindBySkipCellAsync(skip.SkipCode);

                    SkipInDto item = new SkipInDto()
                    {
                        SkipCode = skip.SkipCode,
                        SkipName = skip.SkipName,
                        SkipCellCode = skip.CellCode,
                        BindCellCounts = cells.Where(t => t.CellStatus != CellStatus.Nohave).Count(),
                        SkipRunStatus = SkipRunStatusHelper.SkipRunStatusToChinese(skip.SkipRunStatus),
                        inSkipStatusCount = await _cellRepository.FindCountByShelfNameAsync(skip.SkipName)
                    };
                    items.Add(item);
                }

                result.Items = items;
                result.TotalCount = items.Count;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        /*
        public async Task<List<StockDto>> GetStocksAsync(StockQueryDto para)
        {
            try
            {
                var stocks = await _stockRepository.GetStocksAsync(
                    para.BoxCode, para.CellCode, para.WarehouseAreaName, para.WarehouseName,
                    para.MaterialCode, para.MaterialNameTip, para.MaterialSpecsTip, para.Barcode,
                    para.Status, para.StockInType, para.StockInDateStart, para.StockInDateEnd,
                    para.CheckType, para.CheckResult, para.CheckNoTip,
                    false);

                List<StockDto> stockDtos = new List<StockDto>();
                foreach (var stock in stocks)
                {
                    StockDto dto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    stockDtos.Add(dto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        */

        public async Task<StockDto> GetStockInCellWithBarcodeAsync(string cellCode, string barcode)
        {
            try
            {
                var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                if (cell == null)
                    throw new Exception($"库位码为{cellCode}的库位不存在");

                //东方电子一期，容器为虚拟容器，容器码就是库位码
                var box = await _boxRepository.FindByBoxCodeAsync(cellCode).ConfigureAwait(false);
                if (box == null)
                    throw new Exception($"容器码为{cellCode}的容器不存在");

                var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode, false);
                if (stock == null)
                    throw new Exception($"库位码为{cellCode}的库位中不存在收料码为{barcode}的库存");

                return new StockDto()
                {
                    Id = stock.Id,
                    Barcode = stock.Barcode,
                    BoxId = stock.BoxData.BoxId,
                    BoxCode = stock.BoxData.BoxCode,
                    BoxName = stock.BoxData.BoxName,
                    CellId = stock.CellData.CellId,
                    CellCode = stock.CellData.CellCode,
                    CellName = stock.CellData.CellName,
                    HouseId = stock.Warehouse.HouseId,
                    HouseCode = stock.Warehouse.HouseCode,
                    HouseName = stock.Warehouse.HouseName,
                    AreaId = stock.Warehouse.AreaId,
                    AreaCode = stock.Warehouse.AreaCode,
                    AreaName = stock.Warehouse.AreaName,
                    TotalCountInTime = stock.TotalCountInTime,
                    Status = stock.Status.ToString(),
                    StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                    BatchCode = stock.BatchCode,
                    BLCode = stock.BLCode,
                    BHCode = stock.BHCode,
                    StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                    MaterialCode = stock.Material.MaterialCode,
                    MaterialName = stock.Material.MaterialName,
                    Specs = stock.Material.Specs,
                    Unit = stock.Material.Unit,
                    ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                    ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                    CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                    CheckOrderCode = stock.CheckData.CheckOrderCode,
                    CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                    CheckNo = stock.CheckData.CheckNo,
                    CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                    CheckType = stock.CheckData.CheckTypeInChs(),
                    CheckResult = stock.CheckData.CheckResultInChs(),
                    PassCnt = stock.CheckData.PassCnt,
                    SupplierCode = stock.Supplier.SupplierCode,
                    SupplierName = stock.Supplier.SupplierName,
                    InspectionStatus = (int)(stock.InspectionStatus ?? 0),
                    InspectionCount = stock.InspectionCount
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<StockDto>> GetStocksInCellAsync(string cellCode)
        {
            try
            {
                var cell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                if (cell == null)
                    throw new Exception($"库位码为{cellCode}的库位不存在");

                var stocks = await _stockRepository.GetByCellIdAsync(cell.Id, false);
                if (stocks == null || stocks.Count == 0)
                    return new List<StockDto>();

                List<StockDto> stockDtos = new List<StockDto>();

                foreach (var stock in stocks)
                {
                    var stockDto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        InspectionCount = stock.InspectionCount,
                        InspectionStatus = (int)(stock.InspectionStatus ?? 0),
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    stockDtos.Add(stockDto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<StockDto>> GetStocksInBoxAsync(string boxCode)
        {
            try
            {
                var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                if (box == null)
                    throw new Exception($"容器为{boxCode}的库位不存在");

                var stocks = await _stockRepository.GetByBoxIdAsync(box.Id, false);
                if (stocks == null || stocks.Count == 0)
                    return new List<StockDto>();

                List<StockDto> stockDtos = new List<StockDto>();

                foreach (var stock in stocks)
                {
                    var stockDto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    stockDtos.Add(stockDto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<List<StockDto>> GetStocksAndCheckInBoxAsync(string boxCode)
        {
            try
            {
                var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                if (box == null)
                    throw new Exception($"容器为{boxCode}的库位不存在");

                var stocks = await _stockRepository.GetByBoxIdAsync(box.Id, false);
                //if (stocks == null || stocks.Count == 0)
                    //return new List<StockDto>();

                List<StockDto> stockDtos = new List<StockDto>();

                foreach (var stock in stocks)
                {
                    var stockDto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName,
                        CheckCount = 0
                    };
                    stockDtos.Add(stockDto);
                }

                List<BarcodeCheck> checks = await _barcodeCheckRepository.GetByBoxAsync(box.Id);
                foreach(BarcodeCheck check in checks)
                {
                    BarcodeList barcode = await _barcodeListRepository.FindByIdAsync(check.BarcodeId);
                    var stockDto = new StockDto()
                    {
                        Barcode = barcode.Barcode,
                        TotalCountInTime = 0,
                        MaterialCode = barcode.Material.MaterialCode,
                        MaterialName = barcode.Material.MaterialName,
                        CheckCount = check.Count
                    };
                    stockDtos.Add(stockDto);
                }


                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<StockDto>> GetStocksInSkipAsync(string skipCode)
        {
            try
            {
                List<Cell> cells= await _cellRepository.FindBySkipCellAsync(skipCode);
                if (cells.Count == 0)
                    throw new Exception($"查询料车库位失败");

                var stocks = await _stockRepository.GetSkipCellStockAsync(cells.Select(t=>t.CellCode).ToList());
                if (stocks == null || stocks.Count == 0)
                    return new List<StockDto>();

                List<StockDto> stockDtos = new List<StockDto>();

                foreach (var stock in stocks)
                {
                    var stockDto = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        TotalPagOrBoxInTime = stock.TotalPagOrBox,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        ProcessNo = stock.ProcessNo,
                        Grade = stock.Grade,
                        BoxNumber = stock.BoxData?.BoxNumber,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    stockDtos.Add(stockDto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<StockDto>> GetBoxsInSkipAsync(string skipCode)
        {
            try
            {
                List<Cell> cells = await _cellRepository.FindBySkipCellAsync(skipCode);
                if (cells.Count == 0)
                    throw new Exception($"查询料车库位失败");
                List<Box> boxs = await _boxRepository.GetByCellsIdAsync(cells.Select(t => t.Id).ToList());
                if (boxs == null || boxs.Count == 0)
                    return new List<StockDto>();
                
                List<StockDto> stockDtos = new List<StockDto>();

                foreach (var box in boxs)
                {
                    var stockDto = new StockDto()
                    {
                        BoxId = box.Id,
                        BoxCode = box.BoxCode,
                        BoxName = box.BoxName,
                        CellId = box.CellData.CellId,
                        CellCode = box.CellData.CellCode,
                        CellName = box.CellData.CellName,
                    };
                    stockDtos.Add(stockDto);
                }

                return stockDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }


        /// <summary>
        /// 物料领用
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> StockReceiptAsync(string boxCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    return new ResponseDto() { success = true, message = "该功能已关闭" };
                    /*
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    Cell cell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);

                    int isAuto = 2;
                    try
                    {
                        //是否自动模式
                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync("http://localhost:327/ecs/GetWorkBinDevType");
                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();

                                Console.WriteLine(responseBody);
                                //JSON数据中的结构有一层额外的嵌套。在result字段中，实际的数据是作为字符串存储的，
                                // 解析JSON数据
                                if (responseBody == "1")
                                {
                                    isAuto = 1;
                                }
                                else if (responseBody == "2")
                                {
                                    isAuto = 2;
                                }
                                else if (responseBody == "0")
                                {
                                    isAuto = 2;
                                }
                                else
                                {
                                    return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                                }
                            }
                            else
                            {
                                return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }


                    if (box.PickOutType == "out" && isAuto==1)
                    {
                        string apiUrl = null;
                        if (box.BoxTypeName == "1")
                        {
                            if (cell.ShelfName.IsNullOrEmpty())
                                return new ResponseDto() { success = false, message = $"容器不在料车上，无法出库" };

                            Skip skip = await _skipRepository.FindBySkipCodeAsync(cell.ShelfName);

                            Cell skipCell = await _cellRepository.FindByCellCodeAsync(skip.CellCode);

                            if (skipCell.WarehouseAreaId != 5 || skipCell.AvailableBoxSpecsNames != "ctuin" || skipCell.CellType != CellType.Skip)
                                return new ResponseDto() { success = false, message = $"所在料车位置错误" };

                            apiUrl = $"http://localhost:327/ecs/GetWorkbinRuntype";
                        }
                        else if (box.BoxTypeName == "2")
                        {
                            apiUrl = $"http://localhost:327/ecs/GetTrayRunType";
                        }
                        else
                        {

                        }
                        

                        //是否出库模式
                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync(apiUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();

                                //JSON数据中的结构有一层额外的嵌套。在result字段中，实际的数据是作为字符串存储的，
                                // 解析JSON数据
                                if (responseBody == "1")
                                {
                                    //入库
                                    return new ResponseDto() { success = false, message = $"输送线入库模式，无法出库" };
                                }
                                else if (responseBody == "2")
                                {
                                    //出库
                                }
                                else
                                {
                                    return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                                }
                            }
                            else
                            {
                                return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                            }
                        }

                        if (box.BoxTypeName == "1")
                        {
                            Cell endCell = await _cellRepository.FindByCellCodeAsync("700020A9501013");

                            await SetAsExecutingAsync(cell, endCell, endCell.ShelfName, box, ManageType.CTUSSXOut).ConfigureAwait(false);
                        }
                        else
                        {
                            Cell endCell = await _cellRepository.FindByCellCodeAsync("700030B1501013");
                            await SetAsExecutingAsync(cell, endCell, null, box, ManageType.LiftSSXOut).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);

                        var checks = await _barcodeCheckRepository.GetByBoxAsync(box.Id);

                        foreach (var check in checks)
                        {
                            await _barcodeCheckRepository.DeleteAsync(check);
                        }

                        foreach (Stock stock in stocks)
                        {
                            box.RemoveStock(stock.Id);
                            await _stockRepository.DeleteAsync(stock);
                        }

                        box.SetNoHave();
                        box.DisBindCell();
                        box.PickOutType = null;
                        await _boxRepository.UpdateAsync(box);

                        if (cell != null)
                        {
                            cell.SetCellStatus(CellStatus.Nohave);
                            await _cellRepository.UpdateAsync(cell);
                        }
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);
                    return new ResponseDto() { success = true, message = "收料成功" };
                    */
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        /// <summary>  
        /// 1、暂存仓可以移动到正常仓（后期再加流程），正常仓移动到暂存仓30号之前暂不做
        /// 2、正常仓移动到待处理仓的库存必须是冻结的（不用加流程了），待处理仓移动到正常仓的库存必须是解冻结的（不用加流程了）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> MoveStockAsync(StockMoveDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var srcCell = await _cellRepository.FindByCellCodeAsync(para.SrcCellCode).ConfigureAwait(false);
                    if (srcCell == null)
                        throw new Exception($"库位码为{para.SrcCellCode}的移库源库位不存在");

                    var srcBox = await _boxRepository.FindByBoxCodeAsync(para.SrcBoxCode).ConfigureAwait(false);
                    if (srcBox == null)
                        throw new Exception($"容器码为{para.SrcBoxCode}的移库源容器不存在");

                    if (0 >= srcCell.CellBoxes.Where(o => o.BoxCode == para.SrcBoxCode).Count())
                        throw new Exception($"库位码为{para.SrcCellCode}的移库源库位内不存在容器码为{para.SrcBoxCode}的容器");

                    var tgtCell = await _cellRepository.FindByCellCodeAsync(para.TgtCellCode).ConfigureAwait(false);
                    if (tgtCell == null)
                        throw new Exception($"库位码为{para.TgtCellCode}的移库目标库位不存在");

                    var tgtBox = await _boxRepository.FindByBoxCodeAsync(para.TgtBoxCode).ConfigureAwait(false);
                    if (tgtBox == null)
                        throw new Exception($"容器码为{para.TgtBoxCode}的移库目标容器不存在");

                    if (0 >= tgtCell.CellBoxes.Where(o => o.BoxCode == para.TgtBoxCode).Count())
                        throw new Exception($"库位码为{para.TgtCellCode}的移库目标库位内不存在容器码为{para.TgtBoxCode}的容器");


                    var srcHouse = await _warehouseRepository.FindByIdAsync(srcCell.WarehouseId).ConfigureAwait(false);
                    if (srcHouse == null)
                        throw new Exception($"库位码为{para.SrcCellCode}的移库源库位携带的仓库Id{srcCell.WarehouseId}无效");

                    if (srcCell.WarehouseAreaId == null)
                        throw new Exception($"源库位{srcCell.CellCode}没用库区信息");

                    WarehouseArea srcArea = srcHouse.GetAreaByAreaId(srcCell.WarehouseAreaId.Value);
                    if (srcArea == null)
                        throw new Exception($"库位码为{para.SrcCellCode}的移库源库位携带的库区Id{srcCell.WarehouseAreaId}无效");



                    var tgtHouse = await _warehouseRepository.FindByIdAsync(tgtCell.WarehouseId).ConfigureAwait(false);
                    if (tgtHouse == null)
                        throw new Exception($"库位码为{para.TgtCellCode}的移库目标库位携带的仓库Id{tgtCell.WarehouseId}无效");

                    if (tgtCell.WarehouseAreaId == null)
                        throw new Exception($"目标库位{tgtCell.CellCode}没用库区信息");

                    WarehouseArea tgtArea = tgtHouse.GetAreaByAreaId(tgtCell.WarehouseAreaId.Value);
                    if (tgtArea == null)
                        throw new Exception($"库位码为{para.TgtCellCode}的移库目标库位携带的库区Id{tgtCell.WarehouseAreaId}无效");


                    var srcStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(srcBox.Id, para.BarcodeToMove).ConfigureAwait(false);
                    if (srcStock == null)
                        throw new Exception($"库位码为{para.SrcCellCode}的移库源库位内不存在收料码为{para.BarcodeToMove}的库存");

                    if (para.MoveCount < 1 || para.MoveCount > srcStock.TotalCountInTime)
                        throw new Exception($"移库数量不能小于1，或者大于当前库存的总数{srcStock.TotalCountInTime}");


                    //正常仓移动到暂存仓30号之前暂不做
                    if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "暂存区")
                        throw new Exception($"不允许从正常区调拨到暂存区");

                    if (srcArea.WarehouseAreaName == "暂存区" &&
                        tgtArea.WarehouseAreaName == "正常区" &&
                        srcStock.Status == StockStatus.Freezing)
                        throw new Exception($"从暂存区调拨到正常区的库存不能是冻结的，当前调拨库存处于冻结状态");

                    //正常仓移动到待处理仓的库存必须是冻结的，待处理仓移动到正常仓的库存必须是解冻结的
                    if (srcArea.WarehouseAreaName == "正常区" &&
                        tgtArea.WarehouseAreaName == "待处理区" &&
                        srcStock.Status != StockStatus.Freezing)
                        throw new Exception($"正常区移动到待处理区的库存必须是冻结的，当前调拨库存未冻结");

                    if (srcArea.WarehouseAreaName == "待处理区" &&
                        tgtArea.WarehouseAreaName == "正常区" &&
                        srcStock.Status == StockStatus.Freezing)
                        throw new Exception($"待处理区移动到正常区的库存必须是非冻结的，当前调拨库存处于冻结状态");

                    if (srcArea.WarehouseAreaName == "暂存区" &&
                        tgtArea.WarehouseAreaName == "待处理区")
                        throw new Exception($"不允许从暂存区调拨到待处理区");

                    if (srcArea.WarehouseAreaName == "待处理区" &&
                        tgtArea.WarehouseAreaName == "暂存区")
                        throw new Exception($"不允许从待处理区调拨到暂存区");

                    int moveType = 0;
                    if (srcArea.WarehouseAreaName == "暂存区" && tgtArea.WarehouseAreaName == "正常区")
                        moveType = 1;
                    else if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "暂存区")
                        moveType = 2;
                    else if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "待处理区")
                        moveType = 3;
                    else if (srcArea.WarehouseAreaName == "待处理区" && tgtArea.WarehouseAreaName == "正常区")
                        moveType = 4;

                    if (moveType != 0)
                        await _localEventBus.PublishAsync(new StockMoveEvent(
                            DateTime.Now,
                            srcStock.Supplier.SupplierCode, srcStock.Supplier.SupplierName,
                            srcStock.Material.MaterialCode, srcStock.Material.MaterialName,
                            srcStock.Material.Specs, srcStock.Material.Unit,
                            srcStock.CheckData.CheckNo, srcStock.Barcode,
                            para.MoveCount, moveType,
                            para.OperatorName
                            )).ConfigureAwait(false);

                    //创建新的库存
                    //Stock newStock = await _stocksManager.CreateStockAsync(
                    //    srcStock.Barcode,
                    //    para.MoveCount,
                    //    new MaterialInfoOfStock(srcStock.Material.MaterialCode, srcStock.Material.MaterialName, srcStock.Material.Specs, srcStock.Material.Unit),
                    //    new CountInfoOfStock(srcStock.ReceiveCount.ReceiveTotalCount, srcStock.ReceiveCount.ReceivePkgOrBoxCount, srcStock.ReceiveCount.CountInOnePkgOrBox),
                    //    new CheckInfoOfStock(srcStock.CheckData.CheckOrderCode, srcStock.CheckData.CheckDate, srcStock.CheckData.CheckNo, srcStock.CheckData.CheckNoBeforeReCheck,
                    //    srcStock.CheckData.CheckType, srcStock.CheckData.CheckResult, srcStock.CheckData.PassCnt),
                    //    new SupplierInfoOfStock(srcStock.Supplier.SupplierCode, srcStock.Supplier.SupplierName),
                    //    srcStock.StockInType, srcStock.BatchCode, srcStock.BLCode, srcStock.BHCode);

                    //领走全部源库存
                    //srcStock.Remove(para.MoveCount); //这句话必须加上，若全部领完，Box更新数据
                    if (srcStock.TotalCountInTime <= 0)
                        await _stockRepository.DeleteAsync(srcStock).ConfigureAwait(false);
                    else
                        await _stockRepository.UpdateAsync(srcStock).ConfigureAwait(false);

                    var tgtStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(tgtBox.Id, para.BarcodeToMove).ConfigureAwait(false);
                    //if (tgtStock == null)
                    //{
                    //    newStock.BindBox(tgtBox.Id, tgtBox.BoxCode, tgtBox.BoxName);
                    //    newStock.BindCell(
                    //        tgtCell.Id, tgtCell.CellCode, tgtCell.CellName,
                    //        tgtArea?.Id, tgtArea?.WarehouseAreaCode, tgtArea?.WarehouseAreaName,
                    //        tgtHouse.Id, tgtHouse.WarehouseCode, tgtHouse.WarehouseName);
                    //    await _stockRepository.InsertAsync(newStock).ConfigureAwait(false);
                    //}
                    //else
                    //{
                    //    tgtStock.CombineStock(newStock);
                    //    await _stockRepository.UpdateAsync(tgtStock).ConfigureAwait(false);
                    //}

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "移库完成" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }
        public async Task<ResponseDto> MoveStockAgvAsync(string boxCode, string barcode, int areaId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var srcBox = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (srcBox == null)
                        throw new Exception($"料箱码{boxCode}的料箱不存在");

                    var srcCell = await _cellRepository.FindByCellCodeAsync(srcBox.CellData.CellCode).ConfigureAwait(false);
                    if (srcCell == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位不存在");

                    CellType cellType = srcCell.CellType;
                    if (areaId == 4)
                    {
                        if (srcBox.BoxTypeName == "1")
                            cellType = CellType.WallCell;
                        else
                            cellType = CellType.Cell;
                    }
                    var tgtCell = await _cellRepository.FirstOrDefaultAsync(t => t.WarehouseAreaId == areaId && t.CellType == cellType
                        && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave);
                    if (tgtCell == null)
                        throw new Exception($"分配该区域库位失败");


                    var srcHouse = await _warehouseRepository.FindByIdAsync(srcCell.WarehouseId).ConfigureAwait(false);
                    if (srcHouse == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位携带的仓库Id{srcCell.WarehouseId}无效");

                    if (srcCell.WarehouseAreaId == null)
                        throw new Exception($"源库位{srcCell.CellCode}没用库区信息");

                    WarehouseArea srcArea = srcHouse.GetAreaByAreaId(srcCell.WarehouseAreaId.Value);
                    if (srcArea == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位携带的库区Id{srcCell.WarehouseAreaId}无效");



                    var tgtHouse = await _warehouseRepository.FindByIdAsync(tgtCell.WarehouseId).ConfigureAwait(false);
                    if (tgtHouse == null)
                        throw new Exception($"库位码为{tgtCell.CellCode}的移库目标库位携带的仓库Id{tgtCell.WarehouseId}无效");

                    if (tgtCell.WarehouseAreaId == null)
                        throw new Exception($"目标库位{tgtCell.CellCode}没用库区信息");

                    WarehouseArea tgtArea = tgtHouse.GetAreaByAreaId(tgtCell.WarehouseAreaId.Value);
                    if (tgtArea == null)
                        throw new Exception($"库位码为{tgtCell.CellCode}的移库目标库位携带的库区Id{tgtCell.WarehouseAreaId}无效");


                    var srcStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(srcBox.Id, barcode).ConfigureAwait(false);
                    if (srcStock == null)
                        throw new Exception($"料箱码为{boxCode}的移库源库位内不存在收料码为{barcode}的库存");

                    var boxStock = await _stockRepository.GetByBoxIdAsync(srcBox.Id).ConfigureAwait(false);
                    if (boxStock.Count != 1 && (tgtCell.WarehouseAreaId == 1 || tgtCell.WarehouseAreaId == 2 || tgtCell.WarehouseAreaId == 3))
                        throw new Exception($"料箱码为{boxCode}的料箱中存在多种库存，不允许直接移库");

                    if(srcBox.BoxTypeName =="1" && tgtArea.WarehouseAreaName =="暂存区")
                        throw new Exception($"CTU不存在暂存区");

                    if(srcArea.WarehouseAreaName == tgtArea.WarehouseAreaName)
                        throw new Exception($"不需要同区域调拨");

                    if (await _agvTaskManager.IsExistBoxTask(srcBox.BoxCode))
                        return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };

                    if (srcCell.RunStatus != CellRunStatus.Enable && srcCell.CellStatus != CellStatus.Have)
                        return new ResponseDto() { success = false, message = $"开始库位状态错误" };

                    //正常仓移动到暂存仓30号之前暂不做
                    if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "暂存区")
                        throw new Exception($"不允许从正常区调拨到暂存区");

                    //正常仓移动到待处理仓的库存必须是冻结的，待处理仓移动到正常仓的库存必须是解冻结的
                    if (srcArea.WarehouseAreaName == "正常区" &&
                        tgtArea.WarehouseAreaName == "待处理区" &&
                        srcStock.Status != StockStatus.Freezing)
                        throw new Exception($"正常区移动到待处理区的库存必须是冻结的，当前调拨库存未冻结");

                    if (srcArea.WarehouseAreaName == "暂存区" &&
                        tgtArea.WarehouseAreaName == "正常区" &&
                        srcStock.Status == StockStatus.Freezing)
                        throw new Exception($"从暂存区调拨到正常区的库存不能是冻结的，当前调拨库存处于冻结状态");

                    if (srcArea.WarehouseAreaName == "暂存区" &&
                        tgtArea.WarehouseAreaName == "待处理区")
                        throw new Exception($"不允许从暂存区调拨到待处理区");

                    if (srcArea.WarehouseAreaName == "待处理区" &&
                        tgtArea.WarehouseAreaName == "正常区" &&
                        srcStock.Status == StockStatus.Freezing)
                        throw new Exception($"待处理区移动到正常区的库存必须是非冻结的，当前调拨库存处于冻结状态");

                    if (srcArea.WarehouseAreaName == "待处理区" &&
                        tgtArea.WarehouseAreaName == "暂存区")
                        throw new Exception($"不允许从待处理区调拨到暂存区");

                    if(srcArea.WarehouseAreaName== "暂存区")
                    {
                        Move move = await _moveRepository.FindByCheckNoEnableAsync(srcStock.CheckData.CheckNo);

                        if(move ==null)
                            throw new Exception($"{srcStock.CheckData.CheckNo}检验编号没有未完成的暂存区调拨单");

                        if (srcStock.TotalCountInTime > move.CountToMove - move.MoveCount && tgtArea.WarehouseAreaName != "周转区")
                            throw new Exception($"{srcStock.CheckData.CheckNo}检验编号的暂存区调拨单剩余调拨数量为{move.CountToMove - move.MoveCount}此物料数量为{srcStock.TotalCountInTime},请发往周转区");

                        if(tgtArea.WarehouseAreaName == "分拨墙")
                        {
                            srcBox.PickOutType = "move";
                            srcBox.PickOutAreaId = srcArea.Id.ToString();
                            await _boxRepository.UpdateAsync(srcBox);
                        }
                        else
                        {
                            move.MoveCount += srcStock.TotalCountInTime;
                            await _moveRepository.UpdateAsync(move);
                        }
                    }

                    int moveType = 0;
                    if (srcArea.WarehouseAreaName == "暂存区" && tgtArea.WarehouseAreaName == "正常区")
                        moveType = 1;
                    else if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "暂存区")
                        moveType = 2;
                    else if (srcArea.WarehouseAreaName == "正常区" && tgtArea.WarehouseAreaName == "待处理区")
                        moveType = 3;
                    else if (srcArea.WarehouseAreaName == "待处理区" && tgtArea.WarehouseAreaName == "正常区")
                        moveType = 4;
                    //下架
                    else if (tgtArea.WarehouseAreaName == "周转区")
                        moveType = 5;


                    ManageType type;
                    if (srcBox.BoxTypeName == "1")
                    {
                        type = ManageType.CTUStockMove;
                    }
                    else if(srcBox.BoxTypeName == "2")
                    {
                        type= ManageType.LiftStockMove;
                    }
                    else
                    {
                        throw new Exception($"容器类型不识别");
                    }
                    await SetAsExecutingAsync(srcCell, tgtCell, null, srcBox, type);
                    await uow.SaveChangesAsync();

                    tgtCell.SetSelected();
                    await _cellRepository.UpdateAsync(tgtCell);
                    await uow.SaveChangesAsync();

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "移库完成" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }
        public async Task<ResponseDto> MoveStocksWallAsync(List<string> boxCode)
        {
            try
            {
                boxCode = boxCode.Distinct().ToList();

                List<string> LXboxs = boxCode.Where(t => t.StartsWith("LX")).ToList();
                List<string> TPboxs = boxCode.Where(t => t.StartsWith("TP")).ToList();


                List<Cell> endLXCells = await _cellRepository.GetNoHaveByAreaCellType(LXboxs.Count(), 4, CellType.WallCell).ConfigureAwait(false);
                if (endLXCells.Count != LXboxs.Count())
                    return new ResponseDto() { success = false, message = $"下架{LXboxs.Count()}个料箱，但周转区只有{endLXCells.Count}个空位" };

                List<Cell> endTPCells = await _cellRepository.GetNoHaveByAreaCellType(TPboxs.Count(), 4, CellType.Cell).ConfigureAwait(false);
                if (endTPCells.Count != TPboxs.Count())
                    return new ResponseDto() { success = false, message = $"下架{TPboxs.Count()}个托盘，但周转区只有{endTPCells.Count}个空位" };

                for (int i = 0; i < LXboxs.Count(); i++)
                {
                    using (var uow = UnitOfWorkManager.Begin(true, true))
                    {
                        Box box = await _boxRepository.FindByBoxCodeAsync(LXboxs[i]);
                        if (box == null)
                            return new ResponseDto() { success = false, message = $"读取容器{LXboxs[i]}的失败" };

                        var startCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                        if (startCell == null)
                            return new ResponseDto() { success = false, message = $"读取容器{box.CellData.CellCode}的库位信息失败" };

                        if (await _agvTaskManager.IsExistBoxTask(box.BoxCode))
                            return new ResponseDto() { success = false, message = $"已有下架任务" };

                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box);

                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endLXCells[i], null, box, ManageType.CTUStockOut).ConfigureAwait(false);
                        await uow.SaveChangesAsync();



                        endLXCells[i].SetSelected();
                        await _cellRepository.UpdateAsync(endLXCells[i]);
                        await uow.SaveChangesAsync();

                        await uow.CompleteAsync().ConfigureAwait(false);
                    }
                }

                for (int i = 0; i < TPboxs.Count(); i++)
                {
                    using (var uow = UnitOfWorkManager.Begin(true, true))
                    {
                        Box box = await _boxRepository.FindByBoxCodeAsync(TPboxs[i]);
                        if (box == null)
                            return new ResponseDto() { success = false, message = $"读取容器{TPboxs[i]}的失败" };

                        var startCell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);
                        if (startCell == null)
                            return new ResponseDto() { success = false, message = $"读取容器{box.CellData.CellCode}的库位信息失败" };

                        if (await _agvTaskManager.IsExistBoxTask(box.BoxCode))
                            return new ResponseDto() { success = false, message = $"已有下架任务" };

                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box);

                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endTPCells[i], null, box, ManageType.LiftStockOut).ConfigureAwait(false);
                        await uow.SaveChangesAsync();


                        endTPCells[i].SetSelected();
                        await _cellRepository.UpdateAsync(endTPCells[i]);
                        await uow.SaveChangesAsync();

                        await uow.CompleteAsync().ConfigureAwait(false);
                    }

                }

                return new ResponseDto() { success = true, message = "移库完成" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        //拆箱
        [UnitOfWork]
        public async Task<ResponseDto> StockDevanningAsync(string boxCode, string barcode,string nextBoxCode, string cellCode,int count)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var srcBox = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (srcBox == null)
                        throw new Exception($"料箱码{boxCode}的料箱不存在");

                    var srcCell = await _cellRepository.FindByCellCodeAsync(srcBox.CellData.CellCode).ConfigureAwait(false);
                    if (srcCell == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位不存在");

                    if (srcBox.CellData.CellCode != srcCell.CellCode)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位内不存在容器码为{boxCode}的容器");

                    var tgtCell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (tgtCell == null)
                        throw new Exception($"库位码为{cellCode}的移库目标库位不存在");

                    var tgtBox = await _boxRepository.FindByBoxCodeAsync(nextBoxCode).ConfigureAwait(false);
                    if (tgtBox == null)
                        throw new Exception($"容器码为{nextBoxCode}的移库目标容器不存在");

                    //if (tgtCell.CellCode != tgtBox.CellData.CellCode)
                    //    throw new Exception($"容器码为{nextBoxCode}的所在库位为{tgtBox.CellData.CellCode}");

                    var srcStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(srcBox.Id, barcode).ConfigureAwait(false);
                    if (srcStock == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位内不存在收料码为{barcode}的库存");

                    if (count < 1 || count > srcStock.TotalCountInTime)
                        throw new Exception($"移库数量不能小于1，或者大于当前库存的总数{srcStock.TotalCountInTime}");


                    //创建新的库存
                    Stock newStock = await _stocksManager.CreateStockAsync(
                        srcStock.Barcode,
                        count,
                            new MaterialInfoOfStock(srcStock.Material.MaterialCode, srcStock.Material.MaterialName, srcStock.Material.Specs, srcStock.Material.Unit, srcStock.Material.FinGoodsList),
                            new CountInfoOfStock(srcStock.ReceiveCount.ReceiveTotalCount, srcStock.ReceiveCount.ReceivePkgOrBoxCount, srcStock.ReceiveCount.CountInOnePkgOrBox),
                            new CheckInfoOfStock(srcStock.CheckData.CheckOrderCode, srcStock.CheckData.CheckDate, srcStock.CheckData.CheckNo, srcStock.CheckData.CheckNoBeforeReCheck, srcStock.CheckData.CheckType, srcStock.CheckData.CheckResult, srcStock.CheckData.PassCnt),
                            new SupplierInfoOfStock(srcStock.Supplier.SupplierCode, srcStock.Supplier.SupplierName, srcStock.Supplier.SupplierBatchCode),
                        srcStock.StockInType,
                        srcStock.Status, srcStock.BatchCode, srcStock.BLCode, srcStock.BHCode);

                    //领走全部源库存
                    srcStock.Remove(count); //这句话必须加上，若全部领完，Box更新数据
                    if (srcStock.TotalCountInTime <= 0)
                        await _stockRepository.DeleteAsync(srcStock).ConfigureAwait(false);
                    else
                        await _stockRepository.UpdateAsync(srcStock).ConfigureAwait(false);


                    if ((await _stockRepository.GetByBoxIdAsync(srcBox.Id)).Count() == 0)
                    {
                        srcBox.SetNoHave();
                        await _boxRepository.UpdateAsync(srcBox);
                    }

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(tgtCell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)tgtCell.WarehouseAreaId);

                    var tgtStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(tgtBox.Id, barcode).ConfigureAwait(false);
                    //检查是否满箱
                    decimal fullRate = await BoxIsFul(tgtBox.Id, newStock.Material.MaterialCode, newStock.TotalCountInTime);
                    if (fullRate > 1)
                    {
                        return new ResponseDto() { success = false, message = $"料箱已满箱" };
                    }
                    if (tgtStock == null)
                    {

                        newStock.BindBox(tgtBox.Id, tgtBox.BoxCode, tgtBox.BoxName);
                        await _stockRepository.InsertAsync(newStock).ConfigureAwait(false);
                    }
                    else
                    {

                        tgtStock.CombineStock(newStock);
                        await _stockRepository.UpdateAsync(tgtStock).ConfigureAwait(false);
                    }

                    tgtBox.PickOutType = "move";
                    tgtBox.PickOutAreaId = "1";
                    tgtBox.BindCell(tgtCell, warehouse, warehouseArea);
                    await _boxRepository.UpdateAsync(tgtBox).ConfigureAwait(false);


                    tgtCell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(tgtCell);



                    await uow.SaveChangesAsync();
                    await _stocksManager.BoxFullRate(tgtBox.Id);
                    await _stocksManager.BoxFullRate(srcBox.Id);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "移库完成" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }
        //合箱
        [UnitOfWork]
        public async Task<ResponseDto> StockMergeAsync(string boxCode,string nextBoxCode,string cellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var srcBox = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (srcBox == null)
                        throw new Exception($"料箱码{boxCode}的料箱不存在");

                    var srcCell = await _cellRepository.FindByCellCodeAsync(srcBox.CellData.CellCode).ConfigureAwait(false);
                    if (srcCell == null)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位不存在");

                    if (srcBox.CellData.CellCode != srcCell.CellCode)
                        throw new Exception($"库位码为{srcBox.CellData.CellCode}的移库源库位内不存在容器码为{boxCode}的容器");

                    var tgtCell = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
                    if (tgtCell == null)
                        throw new Exception($"库位码为{cellCode}的移库目标库位不存在");

                    var tgtBox = await _boxRepository.FindByBoxCodeAsync(nextBoxCode).ConfigureAwait(false);
                    if (tgtBox == null)
                        throw new Exception($"容器码为{nextBoxCode}的移库目标容器不存在");

                    if (tgtCell.CellCode != tgtBox.CellData.CellCode)
                        throw new Exception($"容器码为{nextBoxCode}的所在库位为{tgtBox.CellData.CellCode}");

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(tgtCell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)tgtCell.WarehouseAreaId);


                    //if (tgtBox.BoxTypeName != "11")
                    //{
                    //    if (!await BoxIsSameTypeCTU(srcBox.Id, tgtBox.Id))
                    //    {
                    //        return new ResponseDto() { success = false, message = $"料箱已有同类型物料或组盘中有同类型物料" };
                    //    }
                    //}

                    if (tgtBox.BoxTypeName != "11")
                    {
                        //检查是否有同类型物料
                        if (tgtBox.BoxTypeName == "1")
                        {
                            if (!await BoxIsSameTypeCTU(srcBox.Id, tgtBox.Id))
                            {
                                return new ResponseDto() { success = false, message = $"料箱已有同类型物料或组盘中有同类型物料" };
                            }
                        }
                        else
                        {
                            if (!await BoxIsSameTypeLift(srcBox.Id, tgtBox.Id))
                            {
                                return new ResponseDto() { success = false, message = $"托盘已有同类型物料或组盘中有同类型物料或超高" };
                            }
                        }

                        //检查是否有不同区域物料
                        //string endArea = await BoxEndArea(srcBox.Id, tgtBox.Id);
                        if(srcBox.PickOutAreaId != tgtBox.PickOutAreaId)
                        {
                            return new ResponseDto() { success = false, message = $"容器内有不同区域的物料" };
                        }
                    }

                    List<Stock> srcStocks = await _stockRepository.GetByBoxIdAsync(srcBox.Id);
                    foreach(Stock srcStock in srcStocks)
                    {
                        //创建新的库存
                        Stock newStock = await _stocksManager.CreateStockAsync(
                            srcStock.Barcode,
                            srcStock.TotalCountInTime,
                            new MaterialInfoOfStock(srcStock.Material.MaterialCode, srcStock.Material.MaterialName, srcStock.Material.Specs, srcStock.Material.Unit, srcStock.Material.FinGoodsList),
                            new CountInfoOfStock(srcStock.ReceiveCount.ReceiveTotalCount, srcStock.ReceiveCount.ReceivePkgOrBoxCount, srcStock.ReceiveCount.CountInOnePkgOrBox),
                            new CheckInfoOfStock(srcStock.CheckData.CheckOrderCode, srcStock.CheckData.CheckDate, srcStock.CheckData.CheckNo, srcStock.CheckData.CheckNoBeforeReCheck, srcStock.CheckData.CheckType, srcStock.CheckData.CheckResult, srcStock.CheckData.PassCnt),
                            new SupplierInfoOfStock(srcStock.Supplier.SupplierCode, srcStock.Supplier.SupplierName, srcStock.Supplier.SupplierBatchCode),
                            srcStock.StockInType,
                            srcStock.Status, srcStock.BatchCode, srcStock.BLCode, srcStock.BHCode);
                        //领走全部源库存
                        srcStock.Remove(srcStock.TotalCountInTime); //这句话必须加上，若全部领完，Box更新数据
                        await _stockRepository.DeleteAsync(srcStock).ConfigureAwait(false);


                        if (tgtBox.BoxTypeName != "11")
                        {
                            //检查是否满箱
                            decimal fullRate = await BoxIsFul(tgtBox.Id, newStock.Material.MaterialCode, newStock.TotalCountInTime);
                            if (fullRate > 1)
                            {
                                return new ResponseDto() { success = false, message = $"料箱已满箱" };
                            }
                        }

                        var tgtStock = await _stockRepository.FindByBoxIdAndBarcodeAsync(tgtBox.Id, srcStock.Barcode).ConfigureAwait(false);
                        if (tgtStock == null)
                        {

                            newStock.BindBox(tgtBox.Id, tgtBox.BoxCode, tgtBox.BoxName);
                            await _stockRepository.InsertAsync(newStock).ConfigureAwait(false);
                        }
                        else
                        {
                            tgtStock.CombineStock(newStock);
                            await _stockRepository.UpdateAsync(tgtStock).ConfigureAwait(false);
                        }
                    }

                    srcBox.SetNoHave();
                    await _boxRepository.UpdateAsync(srcBox);

                    tgtBox.PickOutType = "move";
                    tgtBox.BindCell(tgtCell, warehouse, warehouseArea);
                    await _boxRepository.UpdateAsync(tgtBox).ConfigureAwait(false);

                    tgtCell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(tgtCell);


                    await uow.SaveChangesAsync();
                    if (tgtBox.BoxTypeName != "11")
                    {
                        await _stocksManager.BoxFullRate(tgtBox.Id);
                    }
                    if(srcBox.BoxTypeName != "11")
                    {
                        await _stocksManager.BoxFullRate(srcBox.Id);
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "移库完成" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<List<StockCheckDto>> GetChecksByBox(string boxcode)
        {
            try
            {
                Box box = await _boxRepository.FindByBoxCodeAsync(boxcode);
                if (box == null)
                    throw new Exception($"容器条码为{boxcode}的容器不存在");

                List<BarcodeCheck> checks = await _barcodeCheckRepository.GetByBoxAsync(box.Id);

                List<StockCheckDto> result = new List<StockCheckDto>();
                foreach (BarcodeCheck check in checks)
                {
                    BarcodeList barcode = await _barcodeListRepository.FindByIdAsync(check.BarcodeId);
                    StockCheckDto dto = new StockCheckDto()
                    {
                        Barcode = barcode.Barcode,
                        StockCode = barcode.Material.MaterialCode,
                        StockName = barcode.Material.MaterialName,
                        count = check.Count
                    };
                    result.Add(dto);

                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<ResponseDto> StockCheckAsync(string barcode, string boxcode, int count)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var barcodeList = await _barcodeListRepository.FindByBarcodeAsync(barcode);
                    if (barcodeList == null)
                        return new ResponseDto() { success = false, message = $"物料条码为{barcode}的物料不存在" };

                    if (barcodeList.isCheckOutCount + count > barcodeList.ReceiveCount.ReceiveTotalCount - barcodeList.InBindCount)
                        return new ResponseDto() { success = false, message = $"抽检数量{count}大于条码总数量{barcodeList.ReceiveCount.ReceiveTotalCount}" };

                    Box box=await _boxRepository.FindByBoxCodeAsync(boxcode);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器条码为{boxcode}的容器不存在" };

                    List<BarcodeCheck> checks=await _barcodeCheckRepository.GetByBoxAsync(box.Id);
                    if(checks.Count > 0)
                    {
                        BarcodeCheck check = checks.FirstOrDefault(t => t.BarcodeId == barcodeList.Id);
                        if (check != null)
                        {
                            barcodeList.isCheckOutCount -= check.Count;
                            check.Count = count;
                            await _barcodeCheckRepository.UpdateAsync(check);
                        }
                        else
                        {
                            check = new BarcodeCheck(box.Id, barcodeList.Id, count);
                            await _barcodeCheckRepository.InsertAsync(check);
                        }
                        barcodeList.isCheckOutCount += count;
                    }
                    else
                    {
                        barcodeList.isCheckOutCount = count;
                        BarcodeCheck check = new BarcodeCheck(box.Id, barcodeList.Id, count);
                        await _barcodeCheckRepository.InsertAsync(check);
                    }

                    barcodeList.isCheckOut = "1";

                    await _barcodeListRepository.UpdateAsync(barcodeList);


                    await _localEventBus.PublishAsync(new StockCheckEvent(barcode, boxcode, count));

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "抽检成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> RemoveStockDirectAsync(Guid stockId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stockExist = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stockExist == null)
                        return new ResponseDto() { success = true, message = $"库存{stockId}原本不存在" };

                    decimal countToDel = stockExist.TotalCountInTime;
                    Guid? cellId = stockExist.CellData.CellId;

                    // 在删除前查询库位中库存数量，用于判断是否需要重置库位状态
                    List<Stock> stocksInCellBeforeDelete = null;
                    if (cellId.HasValue)
                    {
                        stocksInCellBeforeDelete = await _stockRepository.GetByCellIdAsync(cellId.Value).ConfigureAwait(false);
                    }

                    // 如果库存关联了料箱，从料箱中移除
                    if (stockExist.BoxData.BoxId.HasValue)
                    {
                        var box = await _boxRepository.FindByBoxIdAsync(stockExist.BoxData.BoxId.Value).ConfigureAwait(false);
                        if (box != null)
                        {
                            box.RemoveStock(stockId);
                            await _boxStockRepository.DeleteAsync(bs => bs.StockId == stockId);
                            await _boxRepository.UpdateAsync(box);
                        }
                    }

                    await _stockRepository.DeleteAsync(stockExist).ConfigureAwait(false);

                    // 若该库存是库位中唯一的一条，删除后库位变为无货
                    if (stocksInCellBeforeDelete != null && stocksInCellBeforeDelete.Count == 1)
                    {
                        var cell = await _cellRepository.FindByIdAsync(cellId.Value).ConfigureAwait(false);
                        if (cell != null)
                        {
                            cell.SetCellStatus(CellStatus.Nohave);
                            await _cellRepository.UpdateAsync(cell);
                        }
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"Id为{stockId}，barcode为{stockExist.Barcode}，物料码为{stockExist.Material.MaterialCode}, 物料名为{stockExist.Material.MaterialName}，数量为{countToDel}的库存被直接手动删除");
                    return new ResponseDto() { success = true, message = $"删除库存{stockId}成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> OutBountStockDirectAsync(Guid stockId, decimal outBoundCount, int? pagOrBoxCount = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stockExist = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stockExist == null)
                        return new ResponseDto() { success = false, message = $"库存{stockId}不存在" };

                    Guid? cellId = stockExist.CellData.CellId;
                    bool willBeFullyConsumed = stockExist.TotalCountInTime <= outBoundCount;

                    // 在删除前查询库位中库存数量，用于判断是否需要重置库位状态
                    List<Stock> stocksInCellBeforeDelete = null;
                    if (willBeFullyConsumed && cellId.HasValue)
                    {
                        stocksInCellBeforeDelete = await _stockRepository.GetByCellIdAsync(cellId.Value).ConfigureAwait(false);
                    }

                    stockExist.Remove(outBoundCount, pagOrBoxCount);

                    if (stockExist.TotalCountInTime == 0)
                    {
                        if (stockExist.BoxData.BoxId.HasValue)
                        {
                            var box = await _boxRepository.FindByBoxIdAsync(stockExist.BoxData.BoxId.Value).ConfigureAwait(false);
                            if (box != null)
                            {
                                box.RemoveStock(stockId);
                                await _boxStockRepository.DeleteAsync(bs => bs.StockId == stockId);
                                await _boxRepository.UpdateAsync(box);
                            }
                        }
                        await _stockRepository.DeleteAsync(stockExist).ConfigureAwait(false);
                    }
                    else
                        await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);

                    _logger.Info($"库存更新成功，准备创建出库历史记录，stockId={stockId}");

                    var material = stockExist.Material;
                    var cellData = stockExist.CellData;
                    var boxData = stockExist.BoxData;
                    var warehouseData = stockExist.Warehouse;

                    _logger.Info($"物料信息: MaterialCode={material?.MaterialCode}, CellCode={cellData?.CellCode}, BoxCode={boxData?.BoxCode}, Warehouse={warehouseData?.HouseCode}");

                    var stockOutHistory = new StockOutHistory(
                        stockExist.Barcode,
                        material.MaterialCode,
                        material.MaterialName,
                        material.Specs,
                        material.Unit,
                        warehouseData?.HouseCode,
                        warehouseData?.HouseName,
                        warehouseData?.AreaCode,
                        warehouseData?.AreaName,
                        cellData?.CellCode,
                        cellData?.CellName,
                        boxData?.BoxCode,
                        boxData?.BoxName,
                        "手动出库",
                        outBoundCount,
                        DateTime.Now,
                        batchNo: stockExist.BatchCode);

                    _logger.Info("StockOutHistory实体创建成功");

                    if (_stockOutHistoryRepository == null)
                    {
                        _logger.Error("stockOutHistoryRepository为null，无法保存出库历史记录");
                    }
                    else
                    {
                        await _stockOutHistoryRepository.InsertAsync(stockOutHistory).ConfigureAwait(false);
                        _logger.Info("出库历史记录已插入仓库");
                    }

                    // 若该库存是库位中唯一的一条，出库后库位变为无货
                    if (stocksInCellBeforeDelete != null && stocksInCellBeforeDelete.Count == 1)
                    {
                        var cell = await _cellRepository.FindByIdAsync(cellId.Value).ConfigureAwait(false);
                        if (cell != null)
                        {
                            cell.SetCellStatus(CellStatus.Nohave);
                            await _cellRepository.UpdateAsync(cell);
                        }
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"Id为{stockId}，barcode为{stockExist.Barcode}，物料码为{stockExist.Material.MaterialCode}, 物料名为{stockExist.Material.MaterialName}，手动出库数量为{outBoundCount}的库存");
                    return new ResponseDto() { success = true, message = $"库存{stockId}手动出库{outBoundCount}成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"出库历史记录创建失败: {ex.Message}");
                    _logger.Error($"异常堆栈: {ex.StackTrace}");
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        /// <summary>
        /// 物料抽检
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="outBoundCount"></param>
        /// <param name="pagOrBoxCount"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> StockInspectionAsync(Guid stockId, decimal outBoundCount, int? pagOrBoxCount = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stockExist = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stockExist == null)
                        return new ResponseDto() { success = false, message = $"库存{stockId}不存在" };

                    // 累积抽检数量
                    stockExist.InspectionCount += outBoundCount;
                    stockExist.InspectionStatus = Stocks.InspectionStatus.InProgressInspection;

                    stockExist.Remove(outBoundCount, pagOrBoxCount);

                    if (stockExist.TotalCountInTime == 0)
                    {
                        if (stockExist.BoxData.BoxId.HasValue)
                        {
                            var box = await _boxRepository.FindByBoxIdAsync(stockExist.BoxData.BoxId.Value).ConfigureAwait(false);
                            if (box != null)
                            {
                                box.RemoveStock(stockId);
                                await _boxStockRepository.DeleteAsync(bs => bs.StockId == stockId);
                                await _boxRepository.UpdateAsync(box);
                            }
                        }
                        await _stockRepository.DeleteAsync(stockExist).ConfigureAwait(false);
                    }
                    else
                        await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);

                    _logger.Info($"物料抽检成功，stockId={stockId}，抽检数量={outBoundCount}，累计抽检={stockExist.InspectionCount}");

                    // 创建抽检历史记录
                    var material = stockExist.Material;
                    var cellData = stockExist.CellData;
                    var boxData = stockExist.BoxData;
                    var warehouseData = stockExist.Warehouse;

                    var stockOutHistory = new StockOutHistory(
                        stockExist.Barcode,
                        material.MaterialCode,
                        material.MaterialName,
                        material.Specs,
                        material.Unit,
                        warehouseData?.HouseCode,
                        warehouseData?.HouseName,
                        warehouseData?.AreaCode,
                        warehouseData?.AreaName,
                        cellData?.CellCode,
                        cellData?.CellName,
                        boxData?.BoxCode,
                        boxData?.BoxName,
                        "物料抽检",
                        outBoundCount,
                        DateTime.Now,
                        batchNo: stockExist.BatchCode);

                    if (_stockOutHistoryRepository != null)
                    {
                        await _stockOutHistoryRepository.InsertAsync(stockOutHistory).ConfigureAwait(false);
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"Id为{stockId}，barcode为{stockExist.Barcode}，物料抽检数量为{outBoundCount}的库存");
                    return new ResponseDto() { success = true, message = $"物料抽检成功，抽检数量{outBoundCount}" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"物料抽检失败: {ex.Message}");
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        /// <summary>
        /// 抽检完成
        /// </summary>
        /// <param name="stockId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> SetInspectionCompletedAsync(Guid stockId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stockExist = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stockExist == null)
                        return new ResponseDto() { success = false, message = $"库存{stockId}不存在" };

                    stockExist.InspectionStatus = Stocks.InspectionStatus.InProgressInspection;
                    await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"库存{stockId}抽检完成，InspectionStatus={stockExist.InspectionStatus}");
                    return new ResponseDto() { success = true, message = "抽检完成" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"设置抽检完成状态失败: {ex.Message}");
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> ConfirmInspectionQualifiedAsync(Guid stockId, decimal qualifiedQty)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stock = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stock == null)
                        return new ResponseDto { success = false, message = $"库存{stockId}不存在" };

                    if (stock.InspectionStatus != InspectionStatus.InProgressInspection)
                        return new ResponseDto { success = false, message = "当前不处于抽检状态" };

                    if (qualifiedQty <= 0)
                        return new ResponseDto { success = false, message = "合格数量必须大于0" };

                    var inspectionCount = stock.InspectionCount ?? 0;
                    if (qualifiedQty > inspectionCount)
                        return new ResponseDto { success = false, message = $"合格数量({qualifiedQty})不能超过抽检数量({inspectionCount})" };

                    var pagOrBoxDelta = stock.ReceiveCount?.CountInOnePkgOrBox > 0
                        ? (int)(qualifiedQty / stock.ReceiveCount.CountInOnePkgOrBox.Value)
                        : 0;
                    stock.CombineStock(qualifiedQty, pagOrBoxDelta);
                    stock.InspectionCount = inspectionCount - qualifiedQty;
                    stock.InspectionStatus = InspectionStatus.InspectionQualified;

                    await _stockRepository.UpdateAsync(stock).ConfigureAwait(false);

                    var material = stock.Material;
                    var cellData = stock.CellData;
                    var boxData = stock.BoxData;
                    var warehouseData = stock.Warehouse;

                    var stockInHistory = new StockInHistory(
                        stock.Barcode,
                        material?.MaterialCode,
                        material?.MaterialName,
                        material?.Specs,
                        material?.Unit,
                        warehouseData?.HouseCode,
                        warehouseData?.HouseName,
                        warehouseData?.AreaCode,
                        warehouseData?.AreaName,
                        cellData?.CellCode,
                        cellData?.CellName,
                        boxData?.BoxCode,
                        boxData?.BoxName,
                        "抽检合格入库",
                        qualifiedQty,
                        DateTime.Now,
                        batchNo: stock.BatchCode);

                    if (_stockInHistoryRepository != null)
                        await _stockInHistoryRepository.InsertAsync(stockInHistory).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"库存{stockId}确认抽检合格，加回数量{qualifiedQty}");
                    return new ResponseDto { success = true, message = "确认合格成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"确认抽检合格失败: {ex.Message}");
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> SetInspectionNotQualifiedAsync(Guid stockId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var stock = await _stockRepository.FindAsync(stockId).ConfigureAwait(false);
                    if (stock == null)
                        return new ResponseDto { success = false, message = $"库存{stockId}不存在" };

                    if (stock.InspectionStatus != InspectionStatus.InProgressInspection)
                        return new ResponseDto { success = false, message = "当前不处于抽检状态" };

                    stock.InspectionStatus = InspectionStatus.InspectionNotQualified;
                    await _stockRepository.UpdateAsync(stock).ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.Info($"库存{stockId}设置抽检不合格");
                    return new ResponseDto { success = true, message = "设置不合格成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error($"设置抽检不合格失败: {ex.Message}");
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<StockDto> FindByCellAndMaterialAsync(string cellCode, string materialCode)
        {
            if (string.IsNullOrWhiteSpace(cellCode) || string.IsNullOrWhiteSpace(materialCode))
                return null;

            var stock = await _stockRepository.FindByCellCodeAndMaterialCodeAsync(cellCode, materialCode).ConfigureAwait(false);
            if (stock == null)
                return null;

            return new StockDto
            {
                Id = stock.Id,
                Barcode = stock.Barcode,
                TotalCountInTime = stock.TotalCountInTime,
                Status = stock.Status.ToString(),
                InspectionStatus = (int)(stock.InspectionStatus ?? 0),
                InspectionCount = stock.InspectionCount,
                CellCode = stock.CellData?.CellCode,
                CellName = stock.CellData?.CellName,
                HouseCode = stock.Warehouse?.HouseCode,
                HouseName = stock.Warehouse?.HouseName,
                AreaCode = stock.Warehouse?.AreaCode,
                AreaName = stock.Warehouse?.AreaName,
                BoxCode = stock.BoxData?.BoxCode,
                BoxName = stock.BoxData?.BoxName,
                BoxNumber = stock.BoxData?.BoxNumber,
                BatchCode = stock.BatchCode,
                MaterialCode = stock.Material?.MaterialCode,
                MaterialName = stock.Material?.MaterialName,
                Specs = stock.Material?.Specs,
                Unit = stock.Material?.Unit
            };
        }

        public async Task<ResponseDto> PushInspectionReportAsync(List<Guid> stockIds)
        {
            try
            {
                if (stockIds == null || stockIds.Count == 0)
                    return new ResponseDto() { success = false, message = "无待推送的抽检记录" };

                var stockList = new List<Stock>();
                foreach (var id in stockIds)
                {
                    var stock = await _stockRepository.FindAsync(id).ConfigureAwait(false);
                    if (stock != null && stock.InspectionCount > 0)
                        stockList.Add(stock);
                }

                if (stockList.Count == 0)
                    return new ResponseDto() { success = false, message = "无有效的抽检记录" };

                var items = stockList.Select(s => new TuTa.Wms.Erp.IDto.LLBJDDataItem
                {
                    AddType = 0,
                    CSourceCode = s.ReceivingMaterialBarcode ?? s.Barcode,
                    CMemo = $"物料抽检-{s.Material?.MaterialName}",
                    CMAKER = "WMS",
                    Details = new List<TuTa.Wms.Erp.IDto.LLBJDDetail>
                    {
                        new TuTa.Wms.Erp.IDto.LLBJDDetail
                        {
                            SourceAutoId = 0,
                            CInvCode = s.Material?.MaterialCode,
                            CBatch = s.BatchCode,
                            FQuantity = s.InspectionCount ?? 0
                        }
                    }
                }).ToList();

                var request = new TuTa.Wms.Erp.IDto.LLBJDAddRequestDto
                {
                    Cmd = "LLBJDAdd",
                    Maker = "WMS",
                    Data = items
                };

                var result = await _erpAsnAppService.PushLLBJDAddAsync(request).ConfigureAwait(false);
                if (result != null && result.Success)
                {
                    _logger.Info($"推送来料报检单成功，共{stockList.Count}条");
                    return new ResponseDto() { success = true, message = "推送来料报检单成功" };
                }

                _logger.Warn($"推送来料报检单失败: {result?.Message}");
                return new ResponseDto() { success = false, message = result?.Message ?? "推送来料报检单失败" };
            }
            catch (Exception ex)
            {
                _logger.Error($"推送来料报检单异常: {ex.Message}");
                return new ResponseDto() { success = false, message = ex.Message };
            }
        }

        /// <summary>
        /// 4A入库区组盘巷道校验（无断档规则，仅4A适用）：
        /// LanePosition 越小越深、越大越靠巷道口外侧。
        /// 允许外侧连续为空时直接组盘内侧（如 4、5 空则可直接组 3）；
        /// 若外侧某仓位已有货，则其与目标位之间不允许存在空仓位（如 5 有货、4 空则不可组 3）。
        /// </summary>
        private async Task<ResponseDto> Validate4ALaneGroupingOrderAsync(Cell cell)
        {
            if (!cell.CellCode.StartsWith("4A", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(cell.LaneToColumn) || !cell.LanePosition.HasValue)
            {
                _logger.Info($"4A库位{cell.CellCode}未配置LaneToColumn/LanePosition，跳过组盘巷道校验");
                return null;
            }

            var targetPosition = cell.LanePosition.Value;
            var laneCells = await _cellRepository.GetListAsync(c =>
                c.LaneToColumn == cell.LaneToColumn && c.LanePosition.HasValue).ConfigureAwait(false);

            if (laneCells.Count == 0)
            {
                _logger.Info($"4A库位{cell.CellCode}巷道列{cell.LaneToColumn}无同列库位，跳过组盘巷道校验");
                return null;
            }

            var cellsByPosition = laneCells.ToDictionary(c => c.LanePosition!.Value);

            foreach (var outerCell in laneCells.Where(c => c.LanePosition!.Value > targetPosition))
            {
                if (!IsLaneCellOccupied(outerCell.CellStatus))
                    continue;

                for (var middlePosition = targetPosition + 1; middlePosition < outerCell.LanePosition!.Value; middlePosition++)
                {
                    if (!cellsByPosition.TryGetValue(middlePosition, out var middleCell))
                        continue;

                    if (!IsLaneCellOccupied(middleCell.CellStatus))
                    {
                        return new ResponseDto
                        {
                            success = false,
                            message = $"巷道外侧仓位{outerCell.CellCode}已有货，中间仓位{middleCell.CellCode}为空，请先组盘{middleCell.CellCode}或清空外侧"
                        };
                    }
                }
            }

            return null;
        }

        private static bool IsLaneCellOccupied(CellStatus status) => status != CellStatus.Nohave;

        /// <summary>
        /// 4A起点入库巷道校验：查找同巷道列前一位（LanePosition+1）。
        /// 若前一位库位不存在，跳过校验，正常创建并下发；
        /// 若存在且已锁定，再检查其最新AGV任务状态。
        /// </summary>
        private async Task<(bool CanCreate, bool DispatchToRcs)> Evaluate4AInboundLaneAsync(Cell startCell)
        {
            if (string.IsNullOrWhiteSpace(startCell.LaneToColumn) || !startCell.LanePosition.HasValue)
            {
                _logger.Info($"4A库位{startCell.CellCode}未配置LaneToColumn/LanePosition，跳过巷道校验，正常创建下发");
                return (true, true);
            }

            var previousPosition = startCell.LanePosition.Value + 1;
            var previousCell = await _cellRepository.FirstOrDefaultAsync(c =>
                c.LaneToColumn == startCell.LaneToColumn && c.LanePosition == previousPosition).ConfigureAwait(false);

            if (previousCell == null)
            {
                _logger.Info($"4A库位{startCell.CellCode}巷道列{startCell.LaneToColumn}不存在前一位库位(LanePosition={previousPosition})，跳过巷道判断，正常创建下发");
                return (true, true);
            }

            if (previousCell.RunStatus != CellRunStatus.Selected)
            {
                _logger.Info($"同巷前列库位{previousCell.CellCode}未锁定，直接下发");
                return (true, true);
            }

            var previousTask = await _agvTaskManager.GetLatestTaskByStartPositionCodeAsync(previousCell.CellCode).ConfigureAwait(false);
            if (previousTask == null)
            {
                _logger.Info($"同巷前列库位{previousCell.CellCode}已锁定但无AGV任务，直接下发");
                return (true, true);
            }

            if (AgvTaskManager.IsPreviousTaskInWaitingQueue(previousTask))
            {
                _logger.Info($"同巷前列库位{previousCell.CellCode}任务状态{previousTask.AgvTaskStatus}，前位未出储位，本次任务待下发");
                return (true, false);
            }

            _logger.Info($"同巷前列库位{previousCell.CellCode}任务状态{previousTask.AgvTaskStatus}，前位已出储位，直接下发");
            return (true, true);
        }

        /// <summary>
        /// 解析4F库位编码：4F + 排(2位) + 列(2位) + 层(2位)，如 4F120101 = 12排1列1层
        /// </summary>
        private static bool TryParse4FCellLocation(string cellCode, out int row, out int column, out int layer)
        {
            row = 0;
            column = 0;
            layer = 0;

            if (string.IsNullOrWhiteSpace(cellCode) || !cellCode.StartsWith("4F") || cellCode.Length < 8)
                return false;

            var locationPart = cellCode.Substring(2);
            if (locationPart.Length < 6)
                return false;

            return int.TryParse(locationPart.Substring(0, 2), out row)
                && int.TryParse(locationPart.Substring(2, 2), out column)
                && int.TryParse(locationPart.Substring(4, 2), out layer);
        }

        /// <summary>
        /// 按排降序、列升序、层升序选取目标库位
        /// </summary>
        private static Cell SelectEndCellBy4FOrdering(IEnumerable<Cell> cells)
        {
            return cells
                .Select(cell =>
                {
                    if (!TryParse4FCellLocation(cell.CellCode, out var row, out var column, out var layer))
                        return null;

                    return new { Cell = cell, Row = row, Column = column, Layer = layer };
                })
                .Where(item => item != null)
                .OrderByDescending(item => item.Row)
                .ThenBy(item => item.Column)
                .ThenBy(item => item.Layer)
                .Select(item => item.Cell)
                .FirstOrDefault();
        }

        private async Task<AgvTask> SetAsExecutingAsync(Cell startCell, Cell endCell, string skipCode, Box box, ManageType type, bool dispatchToRcs = true, string taskTypOverride = null)
        {
            startCell.SetSelected();
            _logger.Info($"{startCell.CellCode}已锁定");
            await _cellRepository.UpdateAsync(startCell);
            endCell.SetSelected();
            _logger.Info($"{endCell.CellCode}已锁定");
            await _cellRepository.UpdateAsync(endCell);

            AgvTask agvtask = null;

            if (type == ManageType.CTUStockIn || type == ManageType.CTUStockMove || type==ManageType.CTUSSXIn)
            {
                agvtask = await _agvTaskManager.CreateCtuStockInByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type, dispatchToRcs, taskTypOverride);
            }
            else if (type == ManageType.CTUStockOut)
            {
                agvtask = await _agvTaskManager.CreateCTUStockOutByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type, null, null, dispatchToRcs, taskTypOverride);
            }
            else if (type == ManageType.LiftStockOut)
            {
                agvtask = await _agvTaskManager.CreateLiftStockOutByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, type, dispatchToRcs, taskTypOverride);
            }
            else if (type == ManageType.LiftStockIn || type == ManageType.LiftStockMove)
            {
                agvtask = await _agvTaskManager.CreateLiftStockInByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type);
            }
            else if (type == ManageType.CTUSSXOut)
            {
                agvtask = await _agvTaskManager.CreateCtuSSXTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type);
            }
            else if (type == ManageType.LiftSSXOut)
            {
                agvtask = await _agvTaskManager.CreateLiftSSXTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type);
            }
           
            //同时创建AGV任务。
            return agvtask;
        }

        private async Task<decimal> BoxIsFul(Guid boxId,List<StockCreateDto> paras)
        {
            decimal isFul = 0;
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            foreach (Stock stock in stocks)
            {
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.FullBoxCount.GetValueOrDefault() == 0)
                    throw new Exception($"物料没有录入满箱数据，请处理");
                isFul += stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
            }
            foreach (StockCreateDto stock in paras)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    throw new Exception($"条码不存在，请处理");
                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeResult.Material.MaterialCode);
                if (material == null || material.FullBoxCount.GetValueOrDefault() == 0)
                    throw new Exception($"物料没有录入满箱数据，请处理" );
                isFul += stock.TotalCount / material.FullBoxCount.GetValueOrDefault();
            }
            return isFul;
            //if (isFul <= 1)
            //    return true;
            //else
            //    return false;
        }

        private async Task<decimal> BoxIsFul(Guid boxId, string materialCode,decimal count)
        {
            decimal isFul = 0;
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            Material material = null;
            foreach (Stock stock in stocks)
            {
                material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.FullBoxCount.GetValueOrDefault() == 0)
                    throw new Exception($"物料没有录入满箱数据，请处理");
                isFul += stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
            }
                material = await _materialRepository.FindByMaterialCodeAsync(materialCode);
                if (material == null || material.FullBoxCount.GetValueOrDefault() == 0)
                    throw new Exception($"物料没有录入满箱数据，请处理");
                isFul += count / material.FullBoxCount.GetValueOrDefault();
            return isFul;
            //if (isFul <= 1)
            //    return true;
            //else
            //    return false;
        }


        private async Task<bool> BoxIsSameTypeCTU(Guid boxId, List<StockCreateDto> paras)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            List<string> materialtype = new List<string>();
            foreach (Stock stock in stocks)
            {
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialtype.Add(material.BindType);
            }
            foreach (StockCreateDto stock in paras)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    return false;
                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeResult.Material.MaterialCode);
                if (material.IsBind == true) continue;
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                if (materialtype.Contains(material.BindType))
                    return false;
                materialtype.Add(material.BindType);
            }
            return true;
        }


        private async Task<bool> BoxIsSameTypeCTU(Guid boxId, Guid boxIdTwo)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            List<string> materialtype = new List<string>();
            foreach (Stock stock in stocks)
            {
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialtype.Add(material.BindType);
            }
            List<Stock> stocks2 = await _stockRepository.GetByBoxIdAsync(boxIdTwo);
            foreach (Stock stock in stocks2)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    return false;
                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeResult.Material.MaterialCode);
                if (material.IsBind == true) continue;
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                if (materialtype.Contains(material.BindType))
                    return false;
                materialtype.Add(material.BindType);
            }
            return true;
        }


        private async Task<bool> BoxIsSameTypeLift(Guid boxId, List<StockCreateDto> paras)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            List<string> materialCode = new List<string>();
            List<string> materialType = new List<string>();
            foreach (Stock stock in stocks)
            {
                if (materialCode.Contains(stock.Material.MaterialCode))
                    return false;
                materialCode.Add(stock.Material.MaterialCode);
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialType.Add(material.BindType);
            }
            foreach (StockCreateDto stock in paras)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    return false;
                if (materialCode.Contains(barcodeResult.Material.MaterialCode))
                    return false;
                materialCode.Add(barcodeResult.Material.MaterialCode);
                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeResult.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialType.Add(material.BindType);
            }

            if (materialType.Contains("外壳汽车产品") && materialType.Where(t => t != "外壳汽车产品").ToList().Count > 1)
            {
                return false;
            }


            Box box = await _boxRepository.FindByBoxIdAsync(boxId);
            if (!materialType.Contains("外壳汽车产品"))
            {
                if (box.Height == (decimal)1.8)
                    return false;
            }
            return true;
        }


        private async Task<bool> BoxIsSameTypeLift(Guid boxId, Guid boxIdTwo)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            List<string> materialCode = new List<string>();
            List<string> materialType = new List<string>();
            foreach (Stock stock in stocks)
            {
                if (materialCode.Contains(stock.Material.MaterialCode))
                    return false;
                materialCode.Add(stock.Material.MaterialCode);
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialType.Add(material.BindType);
            }
            List<Stock> stocks2 = await _stockRepository.GetByBoxIdAsync(boxIdTwo);
            foreach (Stock stock in stocks2)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    return false;
                if (materialCode.Contains(barcodeResult.Material.MaterialCode))
                    return false;
                materialCode.Add(barcodeResult.Material.MaterialCode);
                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeResult.Material.MaterialCode);
                if (material == null || material.BindType.IsNullOrEmpty())
                    return false;
                materialType.Add(material.BindType);
            }

            if (materialType.Contains("外壳汽车产品") && materialType.Where(t => t != "外壳汽车产品").ToList().Count > 1)
            {
                return false;
            }


            Box box = await _boxRepository.FindByBoxIdAsync(boxId);
            if (!materialType.Contains("外壳汽车产品"))
            {
                if (box.Height == (decimal)1.8)
                    return false;
            }
            return true;
        }

        private async Task<string> BoxEndArea(Guid boxId, List<StockCreateDto> paras)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            string area = null;
            foreach (Stock stock in stocks)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    throw new Exception($"收料码为{stock.Barcode}的收料条目不存在");
                if (area == null)
                {
                    area = barcodeResult.Warehouse.TargetWarehouseCode;
                }
                else
                {
                    if(area != barcodeResult.Warehouse.TargetWarehouseCode)
                        throw new Exception($"有不同区域的物料，无法组盘");
                }
            }
            foreach (StockCreateDto stock in paras)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    throw new Exception($"收料码为{stock.Barcode}的收料条目不存在");
                if (area == null)
                {
                    area = barcodeResult.Warehouse.TargetWarehouseCode;
                }
                else
                {
                    if (area != barcodeResult.Warehouse.TargetWarehouseCode)
                        throw new Exception($"有不同区域的物料，无法组盘");
                }
            }
            return area;
        }

        private async Task<string> BoxEndArea(Guid boxId, Guid boxIdTwo)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId);
            string area = null;
            foreach (Stock stock in stocks)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    throw new Exception($"收料码为{stock.Barcode}的收料条目不存在");
                if (area == null)
                {
                    area = barcodeResult.Warehouse.TargetWarehouseCode;
                }
                else
                {
                    if (area != barcodeResult.Warehouse.TargetWarehouseCode)
                        throw new Exception($"有不同区域的物料，无法组盘");
                }
            }
            List<Stock> stocks2 = await _stockRepository.GetByBoxIdAsync(boxIdTwo);
            foreach (Stock stock in stocks2)
            {
                BarcodeList barcodeResult = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode).ConfigureAwait(false);
                if (barcodeResult == null)
                    throw new Exception($"收料码为{stock.Barcode}的收料条目不存在");
                if (area == null)
                {
                    area = barcodeResult.Warehouse.TargetWarehouseCode;
                }
                else
                {
                    if (area != barcodeResult.Warehouse.TargetWarehouseCode)
                        throw new Exception($"有不同区域的物料，无法组盘");
                }
            }
            return area;
        }

        [UnitOfWork]
        public async Task<ResponseDto> CreateStockDirectAndBindBoxAsync(StockDirectCreateDto stockInfo, string boxCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    // 查找容器
                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"容器码为{boxCode}的容器不存在" };

                    // 检查参数
                    if (stockInfo == null)
                        return new ResponseDto() { success = false, message = "库存信息不能为空" };

                    if (string.IsNullOrEmpty(stockInfo.MaterialCode))
                        return new ResponseDto() { success = false, message = "物料编码不能为空" };

                    if (stockInfo.TotalCount <= 0)
                        return new ResponseDto() { success = false, message = "库存数量必须大于0" };

                    // 生成唯一的收料条码
                    string barcode = $"DIRECT_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                    // 创建库存信息
                    MaterialInfoOfStock material = new MaterialInfoOfStock(
                        stockInfo.MaterialCode,
                        stockInfo.MaterialName ?? stockInfo.MaterialCode,
                        stockInfo.Specs ?? "",
                        stockInfo.Unit ?? "个",
                        null); // 没有成品清单

                    CountInfoOfStock countInfo = new CountInfoOfStock(
                        stockInfo.TotalCount,
                        (int?)stockInfo.TotalCount, // 包装数量等于总数，转换为int?
                        1); // 每包装1个

                    SupplierInfoOfStock supplierInfo = new SupplierInfoOfStock(
                        stockInfo.SupplierCode ?? "DIRECT",
                        stockInfo.SupplierName ?? "直接入库",
                        stockInfo.SupplierBatchCode ?? "");

                    // 创建库存
                    var stock = await _stocksManager.CreateStockAsync(
                        barcode,
                        stockInfo.TotalCount,
                        material,
                        countInfo,
                        supplierInfo,
                        stockInfo.StockInType,
                        StockStatus.Available, // 直接设置状态为可用
                        stockInfo.BatchCode ?? "",
                        stockInfo.BLCode ?? "",
                        stockInfo.BHCode ?? "");

                    // 绑定库存到容器
                    stock.BindBox(box.Id, box.BoxCode, box.BoxName);
                    
                    // 如果容器已绑定库位，也绑定库存到库位
                    if (box.CellData?.CellCode != null)
                    {
                        Cell cell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode);
                        if (cell != null)
                        {
                            Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                            WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);
                            stock.BindCell(cell, warehouse, warehouseArea);
                        }
                    }

                    await _stockRepository.InsertAsync(stock).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);
                    return new ResponseDto() { success = true, message = "直接创建库存并绑定容器成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

    }
}
