using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.Departments;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Dtos;
using TuTa.Wms.PickLists.Entities;
using TuTa.Wms.PickLists.ValueObjects;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Aggregates;
using TuTa.Wms.RecheckLists.Entities;
using TuTa.Wms.RecheckLists.Events;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Wms.ConfigTool;
using Wms.LogTool;

namespace TuTa.Wms.PickLists
{
    public class PickListService : WmsAppService, IPickListService
    {
        private readonly IPickListRepository _pickListRepository;
        private readonly PickListManager _pickListManager;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IErpPickManRepository _eprpPickManRepository;
        private readonly ISkipRepository _skipRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly IRecheckListRepository _recheckListRepository;
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly IRecheckItemRepository _recheckItemRepository;
        private readonly IErpDeptTypeDetailRepository _erpDeptTypeDetailRepository;
        private readonly IErpDeptTypeRepository _erpDeptTypeRepository;
        private readonly StocksManager _stocksManager;
        private readonly AgvTaskManager _agvTaskManager;
        private readonly IPickItemRepository _pickItemRepository;
        private readonly LocalEventBus _localEventBus;
        private readonly ILogger<PickListService> _logger;

        public PickListService(
            IPickListRepository pickNotifierRepository,
            PickListManager pickListManager,
            IDepartmentRepository departmentRepository,
            IMaterialRepository materialRepository,
            IStockRepository stockRepository,
            IBoxRepository boxRepository,
            ICellRepository cellRepository,
            IWarehouseRepository warehouseRepository,
            IErpPickManRepository erpPickManRepository,
            ISkipRepository skipRepository,
            IBarcodeListRepository barcodeListRepository,
            IRecheckListRepository recheckListRepository,
            IChkResultListRepository chkResultListRepository,
            IRecheckItemRepository recheckItemRepository,
            IErpDeptTypeDetailRepository erpDeptTypeDetailRepository,
            IErpDeptTypeRepository erpDeptTypeRepository,
            AgvTaskManager agvTaskManager,
            StocksManager stocksManager,
            IPickItemRepository pickItemRepository,
            LocalEventBus localEventBus,
            ILogger<PickListService> logger)
        {
            _pickListRepository = pickNotifierRepository;
            _pickListManager = pickListManager;
            _departmentRepository = departmentRepository;
            _materialRepository = materialRepository;
            _stockRepository = stockRepository;
            _boxRepository = boxRepository;
            _cellRepository = cellRepository;
            _warehouseRepository = warehouseRepository;
            _eprpPickManRepository = erpPickManRepository;
            _skipRepository = skipRepository;
            _barcodeListRepository = barcodeListRepository;
            _recheckListRepository = recheckListRepository;
            _chkResultListRepository = chkResultListRepository;
            _recheckItemRepository = recheckItemRepository;
            _erpDeptTypeDetailRepository = erpDeptTypeDetailRepository;
            _erpDeptTypeRepository = erpDeptTypeRepository;
            _agvTaskManager = agvTaskManager;
            _stocksManager = stocksManager;
            _pickItemRepository = pickItemRepository;
            _localEventBus = localEventBus;
            _logger = logger;
        }

        public async Task<int> GetUnFinishedPickItmCountAsync()
        {
            try
            {
                List<PickList> pickLists = await _pickListRepository.GetAllUnFinishedPickListsAsync(false).ConfigureAwait(false);

                if (pickLists == null || pickLists.Count == 0)
                    return 0;


                int count = 0;
                foreach (var pickList in pickLists)
                {
                    foreach (var item in pickList.PickItems)
                    {
                        if (item.Status == PickItemStatus.Created || item.Status == PickItemStatus.Picking)
                            count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<PickItemDto>> GetUnFinishedPickItemsAsync(PickItemQueryDto para)
        {
            try
            {
                List<PickList> unFinishedPickLists = await _pickListRepository.GetAllUnFinishedPickListsAsync(false).ConfigureAwait(false);

                List<PickList> pickLists = new List<PickList>();
                if (para.DepartmentId != null)
                //{
                //    pickLists = unFinishedPickLists.Where(o => o.Picker.DeptCode == null).ToList();
                //    pickLists = await _pickListRepository.GetPickListsByDepartmentCodeAsync(
                //    null,
                //    false)
                //    .ConfigureAwait(false);
                //}
                //else
                {
                    var department = await _departmentRepository.FindByIdAsync(para.DepartmentId.Value, false).ConfigureAwait(false);
                    if (department == null)
                        throw new Exception($"Id为{para.DepartmentId}的部门信息不存在");

                    pickLists = unFinishedPickLists.Where(o => o.Picker.DeptCode == department.DepartmentCode).ToList();
                    //pickLists = await _pickListRepository.GetPickListsByDepartmentCodeAsync(
                    //    department.DepartmentCode,
                    //    false)
                    //    .ConfigureAwait(false);
                }
                else
                    pickLists = unFinishedPickLists;
                 
                if (pickLists == null || pickLists.Count == 0)
                    return new List<PickItemDto>();

                if (para.PickType != null)
                    pickLists = pickLists.Where(o => o.Type == para.PickType).ToList();


                var items = new List<PickItemDto>();
                foreach ( var pickList in pickLists) 
                {
                    foreach(var item in pickList.PickItems)
                    {
                        if (item.Status == PickItemStatus.Created || item.Status == PickItemStatus.Picking)
                        {
                            //var stock = await _stockRepository.FindEarliestStockWithMaterialCodeAsync(item.MaterialCode, false);
                            var stockList = await _stockRepository.GetByMaterialCodeAsync(item.MaterialCode, false).ConfigureAwait(false);
                            Stock stock = null;
                            if (para.PickType == 19)
                                stock = stockList.FirstOrDefault(o => o.Warehouse.AreaCode == "002");
                            else
                                stock = stockList.FirstOrDefault(o => o.Status == StockStatus.Available && o.Warehouse.AreaCode == "001");

                            PickItemDto itemDto = new PickItemDto()
                            {
                                PickListId = pickList.Id,
                                PickItemId = item.Id,
                                PickListCode = pickList.PickListCode,
                                PickListDate = pickList.PickListDate.ToString("yyyy-MM-dd"),
                                PickType = PickTypeHelper.PickTypeToChinese(pickList.Type),
                                PickTypeNo = pickList.Type,
                                DeptCode = pickList.Picker.DeptCode,
                                DeptName = pickList.Picker.DeptName,
                                GysCode = pickList.Picker.GysCode,
                                GysName = pickList.Picker.GysName,
                                PickManName = pickList.Picker.PickManName,
                                PickBatch = pickList.PickBatch,
                                GoodsCode = pickList.Goods.GoodsCode,
                                GoodsName = pickList.Goods.GoodsName,
                                GoodsSpecs = pickList.Goods.GoodsSpecs,
                                UniqueCode = item.UniqueCode,
                                MaterialCode = item.MaterialCode,
                                MaterialName = item.MaterialName,
                                Specs = item.Specs,
                                Unit = item.Unit,
                                CountToPick = item.CountToPick,
                                PickedCount = item.PickedCount,
                                PickItemStatus = item.Status.ToString(),
                                CellCode = stock?.CellData.CellCode,
                                CountInCell = stock?.TotalCountInTime
                            };
                            items.Add(itemDto);
                        }
                    }
                }

                //items = items
                //    .Where(o =>
                //    (para.BatchInfo == null ? true : o.PickBatch.Contains(para.BatchInfo)) &&
                //    ((para.MaterialInfo == null ? true : o.MaterialName.Contains(para.MaterialInfo)) ||
                //    (para.MaterialInfo == null ? true : o.Specs.Contains(para.MaterialInfo)))).ToList();

                items = items
                    .Where(o =>
                    (para.QueryBy == 1 ? (para.MaterialCode == null ? true : o.MaterialCode.Contains(para.MaterialCode)) : true) &&
                    (para.QueryBy == 2 ? (para.MaterialNameTip == null ? true : o.MaterialName.Contains(para.MaterialNameTip)) : true) &&
                    (para.QueryBy == 3 ? (para.MaterialSpecsTip == null ? true : o.Specs.Contains(para.MaterialSpecsTip)) : true) &&
                    (para.QueryBy == 4 ? (para.BatchTip == null ? true : o.PickBatch.Contains(para.BatchTip)) : true)).ToList();

                if (para.OrderBy == 1)
                    return items.OrderBy(o => o.MaterialName).ToList();
                else if (para.OrderBy == 2)
                    return items.OrderBy(o => o.PickBatch).ToList();
                else
                    return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<PickItemDto>> GetPagedUnFinishedPickItemsAsync(PagedPickItemQueryDto para)
        {
            try
            {
                if (para.PageIndex <= 0)
                    throw new Exception("分页号无效，必须大于0");

                if (para.PageSize <= 0)
                    throw new Exception("每页数量无效，必须大于0");

                List<PickList> unFinishedPickLists = await _pickListRepository.GetAllUnFinishedPickListsAsync(false).ConfigureAwait(false);

                List<PickList> pickLists = new List<PickList>();
                if (para.DepartmentId != null)
                {
                    var department = await _departmentRepository.FindByIdAsync(para.DepartmentId.Value, false).ConfigureAwait(false);
                    if (department == null)
                        throw new Exception($"Id为{para.DepartmentId}的部门信息不存在");

                    pickLists = unFinishedPickLists.Where(o => o.Picker.DeptCode == department.DepartmentCode).ToList();
                    //pickLists = await _pickListRepository.GetPickListsByDepartmentCodeAsync(
                    //    department.DepartmentCode,
                    //    false)
                    //    .ConfigureAwait(false);
                }
                else
                    pickLists = unFinishedPickLists;

                if (pickLists == null || pickLists.Count == 0)
                    return new PagedResultDto<PickItemDto>() { TotalCount = 0, Items = new List<PickItemDto>() } ;

                //if (para.PickType != null)
                //    pickLists = pickLists.Where(o => o.Type == para.PickType).ToList();


                if (pickLists == null || pickLists.Count == 0)
                    return new PagedResultDto<PickItemDto>() { TotalCount = 0, Items = new List<PickItemDto>() };


                var items = new List<PickItemDto>();
                foreach (var pickList in pickLists)
                {
                    foreach (var item in pickList.PickItems)
                    {
                        if (item.Status == PickItemStatus.Created || item.Status == PickItemStatus.Picking)
                        {
                            PickItemDto itemDto = new PickItemDto()
                            {
                                PickListId = pickList.Id,
                                PickItemId = item.Id,
                                PickListCode = pickList.PickListCode,
                                PickListDate = pickList.PickListDate.ToString("yyyy-MM-dd"),
                                PickType = PickTypeHelper.PickTypeToChinese(pickList.Type),
                                PickTypeNo = pickList.Type,
                                DeptCode = pickList.Picker.DeptCode,
                                DeptName = pickList.Picker.DeptName,
                                GysCode = pickList.Picker.GysCode,
                                GysName = pickList.Picker.GysName,
                                PickManName = pickList.Picker.PickManName,
                                PickBatch = pickList.PickBatch,
                                GoodsCode = pickList.Goods.GoodsCode,
                                GoodsName = pickList.Goods.GoodsName,
                                GoodsSpecs = pickList.Goods.GoodsSpecs,
                                UniqueCode = item.UniqueCode,
                                MaterialCode = item.MaterialCode,
                                MaterialName = item.MaterialName,
                                Specs = item.Specs,
                                Unit = item.Unit,
                                CountToPick = item.CountToPick,
                                PickedCount = item.PickedCount,
                                CheckNo = item.CheckNo,
                                PickItemStatus = item.Status.ToString(),
                                CellCode = null, // stock?.CellData.CellCode,
                                CountInCell = null //stock?.TotalCountInTime
                            };
                            items.Add(itemDto);
                        }
                    }
                }

                items = items
                    .Where(o =>
                    (para.QueryBy == 1 ? (para.MaterialCode == null ? true : o.MaterialCode.Contains(para.MaterialCode)) : true) &&
                    (para.QueryBy == 2 ? (para.MaterialNameTip == null ? true : o.MaterialName.Contains(para.MaterialNameTip)) : true) &&
                    (para.QueryBy == 3 ? (para.MaterialSpecsTip == null ? true : o.Specs.Contains(para.MaterialSpecsTip)) : true) &&
                    (para.QueryBy == 4 ? (para.BatchTip == null ? true : o.PickBatch.Contains(para.BatchTip)) : true)).ToList();

                if (para.OrderBy == 1)
                    items = items.OrderBy(o => o.MaterialName).ToList();
                else if (para.OrderBy == 2)
                    items = items.OrderBy(o => o.PickBatch).ToList();


                if (para.SkipCount >= items.Count)    //para.SkipCount大于等于0
                    return new PagedResultDto<PickItemDto>() { TotalCount = items.Count, Items = new List<PickItemDto>() };
                else
                {
                    List<PickItemDto> result = items.GetRange(
                        para.SkipCount, 
                        items.Count - para.SkipCount >= para.PageSize ? para.PageSize : items.Count - para.SkipCount);

                    foreach(var item in result)
                    {
                        //var stock = await _stockRepository.FindEarliestStockWithMaterialCodeAsync(item.MaterialCode, false);
                        var stockList = await _stockRepository.GetByMaterialCodeAsync(item.MaterialCode, false).ConfigureAwait(false);
                        Stock stock = null;
                        if (para.PickType == 19)
                            //stock = stockList.FirstOrDefault(o => o.Warehouse.AreaCode == "002");
                            if(item.CheckNo != null)
                            {
                                stock = stockList.FirstOrDefault(o => o.Status == StockStatus.Freezing && o.CheckData.CheckNo == item.CheckNo);
                            }
                            else
                            {
                                stock = stockList.FirstOrDefault(o => o.Status == StockStatus.Freezing);
                            }
                        else
                            stock = stockList.FirstOrDefault(o => o.Status == StockStatus.Available && o.Warehouse.AreaCode == "001");
                        item.CellCode = stock?.CellData.CellCode;
                        item.CountInCell = stock?.TotalCountInTime;
                    }

                    return new PagedResultDto<PickItemDto> { TotalCount = items.Count, Items = result };
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }



        /// <summary>
        /// 为某个领料单的某种物料分配领料库存
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<List<PickStockDto>> AllocatePickStocksAsync(PickStockAllocateDto para)
        {
            try
            {
                //查询领料单
                var pickList = await _pickListRepository.FindByPickListCodeAsync(para.PickListCode).ConfigureAwait(false);
                if (pickList == null)
                    //throw new Exception($"不存在单号为{para.PickListCode}的领料单");
                    return new List<PickStockDto>();

                //查询物料对应的领料项
                var pickItem = pickList.GetPickItemByUniqueCode(para.UniqueCode);
                if (pickItem == null)
                    //throw new Exception($"单号为{para.PickListCode}的领料单中不存在UniqueCode为{para.UniqueCode}的领料项");
                    return new List<PickStockDto>();

                pickItem.RemovePickStocks(); //先全部删除，，准备重新分配


                if (pickItem.CountToPick <= pickItem.PickedCount) //该领料项已经领料完成，不分配库存
                {
                    await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                    return new List<PickStockDto>();
                }


                //查询其它领料单中对当前物料的库存分配
                var pickStocksOfMaterialInOtherPickList = await _pickListManager.GetAllPickStocksOfMaterialAsync(pickItem.MaterialCode).ConfigureAwait(false);             
                
                if (pickStocksOfMaterialInOtherPickList == null)
                    pickStocksOfMaterialInOtherPickList = new List<PickStock>();

                pickStocksOfMaterialInOtherPickList = pickStocksOfMaterialInOtherPickList.Where(o => o.PickItemId != pickItem.Id).ToList();


                //从库存中查询指定物料的所有库存实体
                var stocks = await _stockRepository.GetByMaterialCodeAsync(pickItem.MaterialCode).ConfigureAwait(false);
                if (stocks == null || stocks.Count == 0) //没有库存，不分配库存
                {
                    await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                    return new List<PickStockDto>();
                }

                //对库存按照检验时间进行升序排序，实现先进先出，库存必须是可用的，且只能从正常区出库（001表示正常区）
                if (pickList.Type == 19) //退供应商领用，需要从 待处理区 出库
                {
                    stocks = stocks.Where(o => o.Warehouse.AreaCode == "001" || o.Warehouse.AreaCode == "002").ToList();
                    if (pickItem.CheckNo != null)
                    {
                        stocks = stocks.Where(o => o.Status == StockStatus.Freezing && o.CheckData.CheckNo == pickItem.CheckNo).ToList();
                    }
                    else
                    {
                        stocks = stocks.Where(o => o.Status == StockStatus.Freezing).ToList();
                    }
                    //stocks = stocks.Where(o => o.Warehouse.AreaCode == "002").ToList(); //.OrderBy(o => o.CheckData.CheckDate).ToList();
                }
                else  //其它出库，从正常区领用
                    stocks = stocks.Where(o => o.Status == StockStatus.Available && o.Warehouse.AreaCode == "001").ToList(); //.OrderBy(o => o.CheckData.CheckDate).ToList();
                
                if (stocks == null || stocks.Count == 0) //没有可用库存，不分配库存
                {
                    await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                    return new List<PickStockDto>();
                }

                List<Stock> stocksInCertainCell = new List<Stock>();
                if (para.PriorityStocks != null && para.PriorityStocks.Count > 0)
                {
                    List<PriorityStock> reExists = new List<PriorityStock>();
                    for(int i = 0; i < para.PriorityStocks.Count; i++)
                    {
                        if (para.PriorityStocks.GetRange(i + 1, para.PriorityStocks.Count - i - 1)
                            .Where(o => o.CellCode == para.PriorityStocks[i].CellCode && 
                            o.CheckNo == para.PriorityStocks[i].CheckNo).Count() > 0)
                        {
                            reExists.Add(para.PriorityStocks[i]);
                        }
                    }

                    if (reExists.Count > 0)
                    {
                        foreach(var item in reExists)
                        para.PriorityStocks.Remove(item);
                    }

                    foreach (var priority in para.PriorityStocks)
                    {
                        var temp = stocks.Where(o => o.CellData.CellCode == priority.CellCode && o.CheckData.CheckNo == priority.CheckNo).ToList();
                        if (temp != null && temp.Count > 0)
                        {
                            stocksInCertainCell.AddRange(temp);
                        }
                    }
                }

                List<Stock> stocksNotInCertainCell = new List<Stock>();
                foreach(var stock in stocks)
                {
                    if (!stocksInCertainCell.Contains(stock))
                        stocksNotInCertainCell.Add(stock);
                }

                stocks.Clear();
                stocks.AddRange(SortStock(stocksInCertainCell));
                stocks.AddRange(SortStock(stocksNotInCertainCell));                

                List<PickStockDto> result = new List<PickStockDto>();

                foreach (var stock in stocks)
                {
                    if (stock.BoxData.BoxId == null)
                    {
                        _logger.Error($"Id为{stock.Id}的库存没有携带容器信息");
                        continue;
                    }

                    var box = await _boxRepository.FindByBoxIdAsync(stock.BoxData.BoxId.Value).ConfigureAwait(false);
                    if (box == null)    
                    {
                        _logger.Error($"Id为{stock.Id}的库存携带的容器Id{stock.BoxData.BoxId.Value}无效");
                        continue;
                    }

                    if (stock.CellData.CellId == null) 
                        continue;

                    var cell = await _cellRepository.FindAsync(stock.CellData.CellId.Value).ConfigureAwait(false);
                    if (cell == null)
                    {
                        _logger.Error($"Id为{stock.Id}的库存携带的库位Id{stock.CellData.CellId.Value}无效");
                        continue;
                    }
                    
                    if (stock.Warehouse.HouseId == null)
                    {
                        _logger.Error($"Id为{stock.Id}的库存携带库位信息，但未携带仓库信息");
                        continue;
                    }

                    var warehouse = await _warehouseRepository.FindByIdAsync(stock.Warehouse.HouseId.Value).ConfigureAwait(false);
                    if (warehouse == null)
                    {
                        _logger.Error($"Id为{stock.Id}的库存携带的仓库Id{stock.Warehouse.HouseId.Value}无效");
                        continue;
                    }

                    WarehouseArea warehouseArea = null;
                    if (stock.Warehouse.AreaId != null)
                    {
                        warehouseArea = warehouse.GetAreaByAreaId(stock.Warehouse.AreaId.Value);
                        if(warehouseArea == null)
                        {
                            _logger.Error($"Id为{stock.Id}的库存携带的库区Id{stock.Warehouse.AreaId.Value}无效");
                            continue;
                        }
                    }

                    var pickStocksOfThisStock = pickStocksOfMaterialInOtherPickList.Where(o => o.StockId == stock.Id).ToList();
                    decimal cntUsedOfThisStock = 0;
                    foreach(var s in pickStocksOfThisStock)
                    {
                        cntUsedOfThisStock += (s.PickCount - s.PickedCount);
                    }

                    if (stock.TotalCountInTime < cntUsedOfThisStock)
                    {
                        _logger.Error($"Id为{stock.Id}的库存数量小于分配到此库存的领料总数量");
                        continue;
                    }

                    if (stock.TotalCountInTime == cntUsedOfThisStock) //该库存数量已经全部分配完成
                    {
                        _logger.Info($"为领用单{para.PickListCode}中唯一码为{para.UniqueCode}的领用项分配库存时，Id为{stock.Id}的库存数量已被其它领用项分配完，无法再分配");
                        continue;
                    }

                    decimal totalPickCountAllocated = 0;
                    foreach (var s in result)
                    {
                        totalPickCountAllocated += (s.PickCount - s.PickedCount);
                    }

                    if ((stock.TotalCountInTime - cntUsedOfThisStock) >= (pickItem.CountToPick - pickItem.PickedCount - totalPickCountAllocated))
                    {
                        PickStockDto stockDto = new PickStockDto()
                        {
                            PickListCode = pickList.PickListCode,
                            UniqueCode = pickItem.UniqueCode,
                            MaterialCode = pickItem.MaterialCode,
                            MaterialName = pickItem.MaterialName,
                            MaterialSpecs = pickItem.Specs,
                            WarehouseName = warehouse.WarehouseName,
                            WarehouseAreaName = warehouseArea == null ? null : warehouseArea.WarehouseAreaName,
                            CellCode = cell.CellCode,
                            BoxCode = box.BoxCode,
                            Barcode = stock.Barcode,
                            CheckOrderCode = stock.CheckData.CheckOrderCode,
                            CheckNo = stock.CheckData.CheckNo,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            StockCount = stock.TotalCountInTime,
                            PickCount = pickItem.CountToPick - pickItem.PickedCount - totalPickCountAllocated,
                            PickedCount = 0
                        };
                        result.Add(stockDto);

                        pickItem.AddPickStock(
                            stock.Id,
                            stockDto.PickCount);

                        break;
                    }
                    else
                    {
                        PickStockDto stockDto = new PickStockDto()
                        {
                            PickListCode = pickList.PickListCode,
                            UniqueCode = pickItem.UniqueCode,
                            MaterialCode = pickItem.MaterialCode,
                            MaterialName = pickItem.MaterialName,
                            MaterialSpecs = pickItem.Specs,
                            WarehouseName = warehouse.WarehouseName,
                            WarehouseAreaName = warehouseArea == null ? null : warehouseArea.WarehouseAreaName,
                            CellCode = cell.CellCode,
                            BoxCode = box.BoxCode,
                            Barcode = stock.Barcode,
                            CheckOrderCode = stock.CheckData.CheckOrderCode,
                            CheckNo = stock.CheckData.CheckNo,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            StockCount = stock.TotalCountInTime,
                            PickCount = stock.TotalCountInTime - cntUsedOfThisStock,
                            PickedCount = 0
                        };
                        result.Add(stockDto);

                        pickItem.AddPickStock(
                            stock.Id,
                            stockDto.PickCount);

                        continue;
                    }
                }
                
                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);

                return result;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        /// <summary>
        /// 释放指定领料单中指定物料的领料建议
        /// </summary>
        /// <param name="pickItemId"></param>
        /// <returns></returns>
        public async Task<ResponseDto> ReleasePickStockAsync(int pickItemId)
        {
            try
            {
                var pickListExist = await _pickListManager.GetPickListByPickItemIdAsync(pickItemId).ConfigureAwait(false);
                if (pickListExist == null)
                    return new ResponseDto() { success = true, message = $"Id为{pickItemId}的领料项不存在，未占用资源" };

                pickListExist.GetPickItemByPickItemId(pickItemId).RemovePickStocks();
                await _pickListRepository.UpdateAsync(pickListExist).ConfigureAwait(false);

                return new ResponseDto() { success = true, message = "释放占用库存成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<GetByBarcodeBoxDto> GetByBarcodeBoxCode(string barcode,string boxCode)
        {
            try
            {
                var box = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                if (box == null)
                    throw new Exception($"容器码为{boxCode}的容器不存在");

                var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id,barcode).ConfigureAwait(false);
                if(stock == null)
                    throw new Exception($"容器中不存在条形码{barcode}的物料");

                List<PickItem> pickItems = await _pickItemRepository.GetByMaterial(stock.Material.MaterialCode);


                GetByBarcodeBoxDto dto = new GetByBarcodeBoxDto();
                List<PickItemDto> items = new List<PickItemDto>();
                PickItemDto pickDto = null; 

                if (box.PickOutType == "pick")
                {
                    var boxList = await _pickListRepository.FindByPickListCodeAsync(box.PickListCode);
                    if (boxList == null)
                        throw new Exception($"该容器未通过下架绑定默认领料单");

                    var pickItem = boxList.GetPickItemByUniqueCode(box.UniqueCode);
                    if (pickItem == null)
                        throw new Exception($"该容器未通过下架绑定默认领料项");

                    if(pickItem.MaterialCode == stock.Material.MaterialCode)
                    {
                        pickDto = new PickItemDto()
                        {
                            PickListId = boxList.Id,
                            PickItemId = pickItem.Id,
                            PickListCode = boxList.PickListCode,
                            PickListDate = boxList.PickListDate.ToString("yyyy-MM-dd"),
                            PickType = PickTypeHelper.PickTypeToChinese(boxList.Type),
                            PickTypeNo = boxList.Type,
                            DeptCode = boxList.Picker.DeptCode,
                            DeptName = boxList.Picker.DeptName,
                            GysCode = boxList.Picker.GysCode,
                            GysName = boxList.Picker.GysName,
                            PickManName = boxList.Picker.PickManName,
                            PickBatch = boxList.PickBatch,
                            GoodsCode = boxList.Goods.GoodsCode,
                            GoodsName = boxList.Goods.GoodsName,
                            GoodsSpecs = boxList.Goods.GoodsSpecs,
                            UniqueCode = pickItem.UniqueCode,
                            MaterialCode = pickItem.MaterialCode,
                            MaterialName = pickItem.MaterialName,
                            Specs = pickItem.Specs,
                            Unit = pickItem.Unit,
                            CountToPick = pickItem.CountToPick,
                            PickedCount = pickItem.PickedCount,
                            PickItemStatus = pickItem.Status.ToString(),
                            CellCode = stock?.CellData.CellCode,
                            CountInCell = stock?.TotalCountInTime,
                            CountInRemaining = pickItem.CountToPick - pickItem.PickedCount
                        };

                        items.Add(pickDto);
                    }

                }

                foreach (var item in pickItems)
                {
                    var pickList = await _pickListRepository.FindByPickListIdAsync(item.PickListId);

                    if (pickDto != null && item.Id == pickDto.PickItemId)
                    {
                        continue;
                    }
                    PickItemDto itemDto = new PickItemDto()
                    {
                        PickListId = pickList.Id,
                        PickItemId = item.Id,
                        PickListCode = pickList.PickListCode,
                        PickListDate = pickList.PickListDate.ToString("yyyy-MM-dd"),
                        PickType = PickTypeHelper.PickTypeToChinese(pickList.Type),
                        PickTypeNo = pickList.Type,
                        DeptCode = pickList.Picker.DeptCode,
                        DeptName = pickList.Picker.DeptName,
                        GysCode = pickList.Picker.GysCode,
                        GysName = pickList.Picker.GysName,
                        PickManName = pickList.Picker.PickManName,
                        PickBatch = pickList.PickBatch,
                        GoodsCode = pickList.Goods.GoodsCode,
                        GoodsName = pickList.Goods.GoodsName,
                        GoodsSpecs = pickList.Goods.GoodsSpecs,
                        UniqueCode = item.UniqueCode,
                        MaterialCode = item.MaterialCode,
                        MaterialName = item.MaterialName,
                        Specs = item.Specs,
                        Unit = item.Unit,
                        CountToPick = item.CountToPick,
                        PickedCount = item.PickedCount,
                        PickItemStatus = item.Status.ToString(),
                        CellCode = stock?.CellData.CellCode,
                        CountInCell = stock?.TotalCountInTime,
                        CountInRemaining = item.CountToPick - item.PickedCount
                    };
                    items.Add(itemDto);
                }
                


                dto.PickDto = pickDto;
                dto.Items = items;
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        /// <summary>
        /// 物料下架
        /// </summary>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <param name="pickListCode"></param>
        /// <param name="uniqueCode"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> PickOutDownAsync(string startCellCode,string endCellCode,string pickListCode, string uniqueCode, string operatorName = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var startCell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (startCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCellCode}的库位不存在" };

                    var endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                    if (endCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{endCellCode}的库位不存在" };

                    var box = await _boxRepository.FindByCellIdAsync(startCell.Id).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCellCode}上不存在容器" };

                    if (await _agvTaskManager.IsExistBoxTask(box.BoxCode))
                        return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };

                    if (startCell.RunStatus != CellRunStatus.Enable && startCell.CellStatus != CellStatus.Have)
                        return new ResponseDto() { success = false, message = $"开始库位状态错误" };
                    if (startCell.CellType != CellType.Cell && startCell.CellType != CellType.CTUCell)
                        return new ResponseDto() { success = false, message = $"开始库位类型错误" };

                    if (box.BoxTypeName == "1")
                    {

                        if (endCell.CellType != CellType.WallCell && endCell.CellType != CellType.SkipCell)
                            return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                        if (endCell.RunStatus != CellRunStatus.Enable && endCell.CellStatus != CellStatus.Nohave)
                            return new ResponseDto() { success = false, message = $"结束库位状态错误" };
                    }
                    else
                    {

                        if (endCell.CellType != CellType.Cell && endCell.CellType != CellType.Skip)
                            return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                        if (endCell.RunStatus != CellRunStatus.Enable && endCell.CellStatus != CellStatus.Have)
                            return new ResponseDto() { success = false, message = $"结束库位状态错误" };
                    }

                    //if (endCell.CellType != CellType.WallCell && endCell.CellType != CellType.SkipCell && endCell.CellType != CellType.Cell && endCell.CellType != CellType.Skip)
                    //    return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                    Skip skip = null;
                    string skipcode = null;
                    int skiptype = 1;
                    if (endCell.CellType == CellType.SkipCell)
                    {
                        skip = await _skipRepository.FindBySkipCodeAsync(endCell.ShelfName).ConfigureAwait(false);
                        if (skip == null)
                            return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                        skiptype = skip.Type;
                        skipcode = skip.SkipCode;
                    }
                    else if (endCell.CellType == CellType.Skip)
                    {
                        skip = await _skipRepository.FindByCellIdAsync(endCell.Id);
                        if (skip == null)
                            return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                        skiptype = skip.Type;
                        skipcode = skip.SkipCode;
                    }


                    var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode);
                    if(pickList == null)
                        return new ResponseDto() { success = false, message = $"领料单{pickListCode}不存在" };

                    var pickItem = pickList.GetPickItemByUniqueCode(uniqueCode);
                    if(pickItem == null)
                        return new ResponseDto() { success = false, message = $"领料单{pickListCode}中不存在领料项{uniqueCode}" };

                    if (endCell.CellType == CellType.SkipCell && skiptype == 1)
                    {
                        var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);
                        if (stocks.Count == 1)
                        {
                            if (stocks.FirstOrDefault().TotalCountInTime > pickItem.CountToPick - pickItem.PickedCount)
                            {
                                return new ResponseDto() { success = false, message = $"该料箱所选物料分配后有剩余，请发往分拨墙或分拣区" };
                            }
                        }
                        else
                        {
                            return new ResponseDto() { success = false, message = $"该点位料箱存在多种物料不允许直接送往料车绑定" };
                        }

                        if (pickList.Type == 1 || pickList.Type == 15)
                        {
                            skip.TargetLocation = pickList.Picker.DeptName;
                            ErpDeptTypeDetail deptdetail = await _erpDeptTypeDetailRepository.FindByDeptMaterial(pickList.Picker.DeptCode, stocks[0].Material.MaterialCode);
                            if (deptdetail != null)
                                skip.TargetCellType = deptdetail.CLCHKLB_ID;
                            skip.SkipRunStatus = SkipRunStatus.OutByWork;
                        }
                        else
                        {
                            box.PickOutType = "out";
                            await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                            skip.SkipRunStatus = SkipRunStatus.OutByWare;
                        }

                        await _skipRepository.UpdateAsync(skip);

                        pickList.Pick(uniqueCode, stocks[0].Id, stocks[0].TotalCountInTime);

                        if (pickList.Status == PickOrderStatus.Finished)
                            await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                        //await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);
                        else
                            await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);

                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, skipcode, box, ManageType.CTUStockOut, pickListCode, uniqueCode).ConfigureAwait(false);
                    }
                    else if (endCell.CellType == CellType.WallCell)
                    {
                        box.PickListCode = pickListCode;
                        box.UniqueCode = uniqueCode;
                        box.PickOutType = "pick";
                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                        await SetAsExecutingAsync(startCell, endCell, skipcode, box, ManageType.CTUStockOut, pickListCode, uniqueCode).ConfigureAwait(false);
                    }
                    else if (endCell.CellType == CellType.Skip)
                    {
                        var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);

                        if (pickList.Type == 1 || pickList.Type == 15)
                        {
                            skip.TargetLocation = pickList.Picker.DeptName;
                            ErpDeptTypeDetail deptdetail = await _erpDeptTypeDetailRepository.FindByDeptMaterial(pickList.Picker.DeptCode, stocks[0].Material.MaterialCode);
                            if (deptdetail != null)
                                skip.TargetCellType = deptdetail.CLCHKLB_ID;
                            skip.SkipRunStatus = SkipRunStatus.OutByWork;
                        }
                        else
                        {
                            box.PickOutType = "out";
                            await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                            skip.SkipRunStatus = SkipRunStatus.OutByWare;
                        }

                        skip.SkipStatus = SkipStatus.Have;
                        await _skipRepository.UpdateAsync(skip);

                        box.PickListCode = pickListCode;
                        box.UniqueCode = uniqueCode;
                        box.PickOutType = "pick";
                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);

                        if (stocks.Count == 1 && (stocks.FirstOrDefault().TotalCountInTime < pickItem.CountToPick - pickItem.PickedCount))
                        {
                            pickList.Pick(uniqueCode, stocks[0].Id, stocks[0].TotalCountInTime);

                            if (pickList.Status == PickOrderStatus.Finished)
                                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                            else
                                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);

                            //通知出库
                            if (pickList != null)
                            {
                                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(Guid.Parse("3a1336e6-d487-822e-1ec1-ccff2bd9b0f5"));
                                WarehouseArea warehouseArea = warehouse.GetAreaByAreaId(int.Parse(box.PickOutAreaId));
                                //出库通知
                                _pickListManager.StockPickOut(stocks.FirstOrDefault(), pickList, pickItem.UniqueCode, operatorName, warehouseArea, stocks.FirstOrDefault().TotalCountInTime);
                            }
                        }


                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.LiftStockOut, pickListCode, uniqueCode).ConfigureAwait(false);
                    }
                    else if(endCell.CellType == CellType.Cell)
                    {
                        box.PickListCode = pickListCode;
                        box.UniqueCode = uniqueCode;
                        box.PickOutType = "pick";
                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                        _logger.Info("开始创建agv任务");

                        await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.LiftStockOut, pickListCode, uniqueCode).ConfigureAwait(false);
                    }
                    else
                    {
                        return new ResponseDto() { success = false, message = $"结束库位类型错误" };
                    }

                    endCell.SetSelected();
                    await _cellRepository.UpdateAsync(endCell);



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

        /// <summary>
        /// 检验下架
        /// </summary>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> CheckDownAsync(string startCellCode, string endCellCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var startCell = await _cellRepository.FindByCellCodeAsync(startCellCode).ConfigureAwait(false);
                    if (startCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCellCode}的库位不存在" };

                    var endCell = await _cellRepository.FindByCellCodeAsync(endCellCode).ConfigureAwait(false);
                    if (endCell == null)
                        return new ResponseDto() { success = false, message = $"库位码为{endCellCode}的库位不存在" };

                    var box = await _boxRepository.FindByCellIdAsync(startCell.Id).ConfigureAwait(false);
                    if (box == null)
                        return new ResponseDto() { success = false, message = $"库位码为{startCellCode}上不存在容器" };

                    //var skip = await _skipRepository.FindBySkipCodeAsync(endCell.ShelfName).ConfigureAwait(false);
                    //if (skip == null)
                    //    return new ResponseDto() { success = false, message = $"读取料车信息失败" };

                    if (await _agvTaskManager.IsExistBoxTask(box.BoxCode))
                        return new ResponseDto() { success = false, message = $"该容器已存在AGV任务" };

                    if (startCell.RunStatus != CellRunStatus.Enable && startCell.CellStatus != CellStatus.Have)
                        return new ResponseDto() { success = false, message = $"开始库位状态错误" };
                    if (startCell.CellType != CellType.Cell && startCell.CellType != CellType.CTUCell)
                        return new ResponseDto() { success = false, message = $"开始库位类型错误" };

                    if (endCell.RunStatus != CellRunStatus.Enable && endCell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"结束库位状态错误" };
                    if (endCell.CellType != CellType.WallCell && endCell.CellType != CellType.SkipCell)
                        return new ResponseDto() { success = false, message = $"结束库位类型错误" };

                    if (endCell.CellType == CellType.WallCell)
                    {
                        //RecheckItem recheck = await _recheckItemRepository.FindByCheckNoAsync()
                        //if ())
                        //box.PickListCode = pickListCode;
                        //box.UniqueCode = uniqueCode;
                        box.PickOutType = "check";
                        box.PickOutAreaId = startCell.WarehouseAreaId.ToString();
                        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                    }
                    else
                    {
                        return new ResponseDto() { success = false, message = $"结束库位类型错误" };
                    }

                    _logger.Info("开始创建agv任务");

                    await SetAsExecutingAsync(startCell, endCell, null, box, ManageType.CTUStockOut, null, null).ConfigureAwait(false);

                    endCell.SetSelected();
                    await _cellRepository.UpdateAsync(endCell);

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

        /// <summary>
        /// 空箱下架
        /// </summary>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> NoHaveDownAsync(int count,string type,string area,string endArea)
        {
            try
            {
                List<Cell> endCells = null;
                List<Cell> noHaveBoxCells = null;

                if (area == "综合")
                    area = null;
                else
                    area = await _erpDeptTypeRepository.GetIdByName(area);

                if (type == "1")
                {
                    endCells = await _cellRepository.GetNoHaveByAreaCellType(count, 4, CellType.WallCell).ConfigureAwait(false);
                    if (endCells.Count != count)
                        return new ResponseDto() { success = false, message = $"下架{count}个空料箱，但周转区只有{endCells.Count}个空位" };


                    noHaveBoxCells = await _cellRepository.GetNoHaveBox(area, CellType.CTUCell);
                }
                else if (type == "2")
                {
                    noHaveBoxCells = await _cellRepository.GetNoHaveBox(area, CellType.Cell);
                    if (endArea == "0")
                    {
                        endCells = await _cellRepository.GetNoHaveByAreaCellType(count, 4, CellType.Cell).ConfigureAwait(false);
                        if (endCells.Count != count)
                            return new ResponseDto() { success = false, message = $"下架{count}个空托盘，但周转区只有{endCells.Count}个空位" };

                    }
                    else
                    {

                        //是否出库模式
                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync("http://192.168.0.4:327/ecs/GetTrayRunType");
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
                    }
                }
                else
                {
                    return new ResponseDto() { success = false, message = $"下架料箱类型错误" };
                }


                List<Box> boxs = await _boxRepository.GetNoHaveInAsync(count, type, noHaveBoxCells.Select(t => t.CellCode).ToList()).ConfigureAwait(false);
                if (boxs.Count != count)
                    return new ResponseDto() { success = false, message = $"下架{count}个空料箱，但库内只有{boxs.Count}个空料箱" };

                for (int i = 0; i < count; i++)
                {
                    using (var uow = UnitOfWorkManager.Begin(true, true))
                    {
                        var startCell = await _cellRepository.FindByCellCodeAsync(boxs[i].CellData.CellCode).ConfigureAwait(false);
                        if (startCell == null)
                            return new ResponseDto() { success = false, message = $"读取容器{boxs[i].CellData.CellCode}的库位信息失败" };

                        if (await _agvTaskManager.IsExistBoxTask(boxs[i].BoxCode))
                            return new ResponseDto() { success = false, message = $"已有下架空容器任务" };


                        _logger.Info("开始创建agv任务");

                        if (type == "1")
                        {
                            await SetAsExecutingAsync(startCell, endCells[i], null, boxs[i], ManageType.CTUStockOut, null, null).ConfigureAwait(false);
                        }
                        else if (type == "2")
                        {
                            if (endArea == "0")
                            {
                                await SetAsExecutingAsync(startCell, endCells[i], null, boxs[i], ManageType.LiftStockOut, null, null).ConfigureAwait(false);
                            }
                            else
                            {

                                Cell endCell = await _cellRepository.FindByCellCodeAsync("700030B1501013").ConfigureAwait(false);
                                if (endCell == null)
                                    return new ResponseDto() { success = false, message = $"库位码为700030B1501013的库位不存在" };
                                await SetAsExecutingAsync(startCell, endCell, null, boxs[i], ManageType.LiftSSXOut, null, null).ConfigureAwait(false);
                            }
                        }
                        await uow.SaveChangesAsync();

                        if(endArea== "0")
                        {
                            endCells[i].SetSelected();
                            await _cellRepository.UpdateAsync(endCells[i]);
                        }
                        await uow.SaveChangesAsync();

                        await uow.CompleteAsync().ConfigureAwait(false);
                    }
                    
                }

                return new ResponseDto() { success = true, message = "创建下架任务成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }


        public async Task<ResponseDto> PickOutByZZ(string barcode,string boxCode,decimal count,string pickListCode,string uniqueCode,string operatorName=null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {

                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode);
                    if (box == null)
                        throw new Exception($"容器码为{boxCode}的容器不存在");

                    var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode);
                    if (stock == null)
                        throw new Exception($"容器{boxCode}中不存在收料码为{barcode}的库存");

                    Console.WriteLine(JsonConvert.SerializeObject(stock));

                    if (stock.TotalCountInTime < count)
                        throw new Exception($"容器{boxCode}中收料码为{barcode}的库存数量不足，只有{stock.TotalCountInTime}，需要出库{count}");

                    if(box.PickOutType == null && box.BoxTypeName !="11")
                        throw new Exception($"容器{boxCode}中不是下架容器");

                    if (box.PickOutType == "pick" || box.BoxTypeName=="11")
                    {
                        if (pickListCode != null && uniqueCode != null)
                        {

                            var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                            if (pickList == null)
                                throw new Exception($"单号为{pickListCode}的领料单不存在");

                            var pickItem = pickList.GetPickItemByUniqueCode(uniqueCode);
                            if (pickItem == null)
                                throw new Exception($"单号为{pickListCode}的领料单中不存在唯一码为{uniqueCode}的领用项");

                            if ((pickItem.CountToPick - pickItem.PickedCount) < count)
                                throw new Exception($"单号为{pickListCode}的领料单中，领料项唯一码为{uniqueCode}的领用项只需要领用{pickItem.CountToPick - pickItem.PickedCount}，实际却准备领{count}");

                            pickList.Pick(uniqueCode, stock.Id, count, "");

                            if (pickList.Status == PickOrderStatus.Finished)
                                //await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);
                                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                            else
                                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);


                            Warehouse warehouse = await _warehouseRepository.FindByIdAsync(Guid.Parse("3a1336e6-d487-822e-1ec1-ccff2bd9b0f5")).ConfigureAwait(false);

                            WarehouseArea warehouseArea = null;
                            if (box.BoxTypeName == "11")
                            {
                                warehouseArea = warehouse.GetAreaByAreaId(1);
                            }
                            else
                            {
                                warehouseArea = warehouse.GetAreaByAreaId(int.Parse(box.PickOutAreaId));
                            }
                            //出库通知
                            _pickListManager.StockPickOut(stock, pickList, pickItem.UniqueCode, operatorName, warehouseArea ,count);





                            stock.Remove(count);
                            if (stock.TotalCountInTime == 0)
                                await _stockRepository.DeleteAsync(stock);
                            else
                                await _stockRepository.UpdateAsync(stock);
                        }
                    }
                    else if (box.PickOutType == "check")
                    {
                        List<ChkResultList> chkList = await _chkResultListRepository.FindByBarcodeAsync(barcode);
                        ChkResultList chk = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);
                        if (chk == null)
                        {
                            chk = chkList.FirstOrDefault();
                            if(chk == null)
                            {
                                throw new Exception($"获取检验单失败");
                            }
                        }

                        RecheckItem item = await _recheckItemRepository.FindByCheckNoAsync(chk.CheckData.CheckNo);

                        if (count > item.CheckCount - item.PickedCount)
                            throw new Exception($"检验编号为{chk.CheckData.CheckNo}的物料只需要领用{item.CheckCount - item.PickedCount}，实际却准备领{count}");
                        item.PickAway(count);
                        await _recheckItemRepository.UpdateAsync(item);

                        BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(barcode);
                        barcodeList.InBindCount -= count;
                        await _barcodeListRepository.UpdateAsync(barcodeList);

                        //复检项出库，同时通知库存进行扣减
                        await _localEventBus.PublishAsync(new ReCheckStockOutEvent()
                        {
                            StockId = stock.Id,
                            Barcode = barcode, //验证用
                            Stock = stock,
                            PickedCount = count,
                            OperatorName = operatorName
                        });
                    }
                    else if(box.PickOutType == "move")
                    {

                    }
                    else
                    {
                        throw new Exception($"下架类型错误");
                    }



                    if ((await _stockRepository.GetByBoxIdAsync(box.Id)).Count() == 0)
                    {
                        box.SetNoHave();
                        await _boxRepository.UpdateAsync(box);
                    }


                    await uow.SaveChangesAsync();
                    if (box.BoxTypeName != "11")
                    {
                        await _stocksManager.BoxFullRate(box.Id);
                    }

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "领用成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> PickOutByBox(string barcode, string boxCode, decimal count, string pickListCode, string uniqueCode,string nextBoxCode,string nextCellCode, string operatorName = null)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                    if (pickList == null)
                        throw new Exception($"单号为{pickListCode}的领料单不存在");

                    var pickItem = pickList.GetPickItemByUniqueCode(uniqueCode);
                    if (pickItem == null)
                        throw new Exception($"单号为{pickListCode}的领料单中不存在唯一码为{uniqueCode}的领用项");

                    var box = await _boxRepository.FindByBoxCodeAsync(boxCode);
                    if (box == null)
                        throw new Exception($"容器码为{boxCode}的容器不存在");

                    var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode).ConfigureAwait(false);
                    if (stock == null)
                        throw new Exception($"容器{boxCode}中不存在收料码为{barcode}的库存");

                    if (stock.TotalCountInTime < count)
                        throw new Exception($"容器{boxCode}中收料码为{barcode}的库存数量不足，只有{stock.TotalCountInTime}，需要出库{count}");

                    if ((pickItem.CountToPick - pickItem.PickedCount) < count)
                        throw new Exception($"单号为{pickListCode}的领料单中，领料项唯一码为{uniqueCode}的领用项只需要领用{pickItem.CountToPick - pickItem.PickedCount}，实际却准备领{count}");

                    var nextbox = await _boxRepository.FindByBoxCodeAsync(nextBoxCode);
                    if (nextbox == null)
                        throw new Exception($"容器码为{nextBoxCode}的容器不存在");

                    /*
                    //var nextcell = await _cellRepository.FindByCellCodeAsync(nextCellCode).ConfigureAwait(false);
                    //if(nextcell == null)
                    //    throw new Exception($"库位码为{nextCellCode}的库位不存在");


                    //if (nextcell.CellType == CellType.SkipCell)
                    //{
                    //    var skip = await _skipRepository.FindBySkipCodeAsync(nextcell.ShelfName).ConfigureAwait(false);
                    //    if (skip == null)
                    //        throw new Exception($"获取库位所属料车失败");

                    //    if (skip.CellId != null)
                    //    {
                    //        var skipCell = await _cellRepository.FindByIdAsync((Guid)skip.CellId);
                    //        if (skipCell.WarehouseAreaId != 4)
                    //            throw new Exception($"库位所属料车不在周转区");
                    //    }
                    //    else
                    //    {
                    //        throw new Exception($"料车未绑定库位");
                    //    }


                    //    if (pickList.Type == 1 || pickList.Type == 15)
                    //    {
                    //        if (pickList.Picker.DeptName != skip.TargetLocation && !skip.TargetLocation.IsNullOrEmpty())
                    //        {
                    //            throw new Exception($"该料车已发往{skip.TargetLocation}无法绑定");
                    //        }
                    //        skip.TargetLocation = pickList.Picker.DeptName;

                    //        skip.SkipRunStatus = SkipRunStatus.OutByWork;
                    //    }
                    //    else
                    //    {
                    //        box.PickOutType = "out";
                    //        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                    //        skip.SkipRunStatus = SkipRunStatus.OutByWare;
                    //    }

                    //    //skip.TargetLocation = pickList.Picker.DeptName;
                    //    //skip.SkipRunStatus = SkipRunStatus.OutByWork;
                    //    await _skipRepository.UpdateAsync(skip);
                    //}
                    //else if (nextcell.CellType == CellType.WallCell)
                    //{

                    //}
                    //else
                    //{
                    //    throw new Exception($"目标库位类型错误");
                    //}
                    */

                    ErpDeptTypeDetail deptdetail = await _erpDeptTypeDetailRepository.FindByDeptMaterial(pickList.Picker.DeptCode, stock.Material.MaterialCode);
                    string materialType = null;
                    if (deptdetail != null)
                        materialType = deptdetail.CLCHKLB_ID;

                    if(nextbox.PickDeptName!=null && nextbox.PickDeptName!=pickList.Picker.DeptName)
                        throw new Exception($"容器目标车间为{nextbox.PickDeptName},当前出库单为{pickList.Picker.DeptName}");

                    if (nextbox.PickWorkType != null && nextbox.PickWorkType != materialType)
                        throw new Exception($"容器目标车间区域为{nextbox.PickWorkType},当前物料为{materialType}");

                    if (pickList.Type == 1 || pickList.Type == 15)
                    {
                        nextbox.PickOutType = "outwork";
                        nextbox.PickWorkType = materialType;
                    }
                    else
                    {
                        nextbox.PickOutType = "outware";
                    }
                    nextbox.PickDeptName = pickList.Picker.DeptName;
                    await _boxRepository.UpdateAsync(nextbox);

                    pickList.Pick(uniqueCode, stock.Id, count);

                    if (pickList.Status == PickOrderStatus.Finished)
                        //await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);
                        await _pickListRepository.UpdateAsync(pickList);
                    else
                        await _pickListRepository.UpdateAsync(pickList);

                    stock.Remove(count);
                    if (stock.TotalCountInTime > 0)
                        await _stockRepository.UpdateAsync(stock);
                    else
                        await _stockRepository.DeleteAsync(stock);

                    if ((await _stockRepository.GetByBoxIdAsync(box.Id)).Count() == 0)
                    {
                        box.SetNoHave();
                        await _boxRepository.UpdateAsync(box);
                    }

                    var nextstock = await _stocksManager.CreateStockAsync(
                        stock.Barcode,
                        count,
                        new MaterialInfoOfStock(stock.Material.MaterialCode, stock.Material.MaterialName, stock.Material.Specs, stock.Material.Unit, stock.Material.FinGoodsList),
                        new CountInfoOfStock(stock.ReceiveCount.ReceiveTotalCount, stock.ReceiveCount.ReceivePkgOrBoxCount, stock.ReceiveCount.CountInOnePkgOrBox),
                        new SupplierInfoOfStock(stock.Supplier.SupplierCode, stock.Supplier.SupplierName, stock.Supplier.SupplierBatchCode),
                        new CheckInfoOfStock(stock.CheckData.CheckOrderCode,stock.CheckData.CheckDate, stock.CheckData.CheckNo, stock.CheckData.CheckNoBeforeReCheck, stock.CheckData.CheckType, stock.CheckData.CheckResult, stock.CheckData.PassCnt),
                        stock.StockInType,
                        stock.BatchCode,
                        stock.BLCode,
                        stock.BHCode);

                    var stockExist = await _stockRepository.FindByBoxIdAndBarcodeAsync(nextbox.Id, stock.Barcode);
                    if (stockExist != null)
                    {
                        stockExist.CombineStock(nextstock);
                        await _stockRepository.UpdateAsync(stockExist);
                    }
                    else
                    {
                        nextstock.BindBox(nextbox.Id, nextbox.BoxCode, nextbox.BoxName);
                        await _stockRepository.InsertAsync(nextstock);
                    }

                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(Guid.Parse("3a1336e6-d487-822e-1ec1-ccff2bd9b0f5"));
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId(int.Parse(box.PickOutAreaId));
                    //出库通知
                    _pickListManager.StockPickOut(nextstock, pickList, pickItem.UniqueCode, operatorName, warehouseArea, count);

                    await uow.SaveChangesAsync();
                    await _stocksManager.BoxFullRate(box.Id);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "领用成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }


        /// <summary>
        /// 针对指定的领料单进行领料
        /// </summary>
        /// <param name="pickListCode">针对的领料单</param>
        /// <param name="pickItemUniqueCode">针对的领料项的唯一码</param>
        /// <param name="para">领料库存来源</param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<ResponseDto> PickOutAsync(string pickListCode, string pickItemUniqueCode, PickOutDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                    if (pickList == null)
                        throw new Exception($"单号为{pickListCode}的领料单不存在");

                    var pickItem = pickList.GetPickItemByUniqueCode(pickItemUniqueCode);
                    if (pickItem == null)
                        throw new Exception($"单号为{pickListCode}的领料单中不存在唯一码为{pickItemUniqueCode}的领用项");

                    var pickStocks = pickItem.GetAllPickStocks();
                    if (pickStocks == null || pickStocks.Count == 0)
                        throw new Exception($"单号为{pickListCode}的领料单中，唯一码为{pickItemUniqueCode}的领用项没有指定从哪个库存领取");

                    var box = await _boxRepository.FindByBoxCodeAsync(para.BoxCode).ConfigureAwait(false);
                    if (box == null)
                        throw new Exception($"容器码为{para.BoxCode}的容器不存在");

                    var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, para.Barcode).ConfigureAwait(false);
                    if (stock == null)
                        throw new Exception($"容器{para.BoxCode}中不存在收料码为{para.Barcode}的库存");

                    var pickStock = pickStocks.FirstOrDefault(o => o.PickItemId == pickItem.Id && o.StockId == stock.Id);
                    if (pickStock == null)
                        throw new Exception($"单号为{pickListCode}的领料单中，领料项唯一码为{pickItemUniqueCode}的领用项没有指定从容器为{para.BoxCode}，收料码为{para.Barcode}的库存中领取");

                    if (stock.TotalCountInTime < para.PickOutCnt)
                        throw new Exception($"容器{para.BoxCode}中收料码为{para.Barcode}的库存数量不足，只有{stock.TotalCountInTime}，需要出库{para.PickOutCnt}");

                    if ((pickItem.CountToPick - pickItem.PickedCount) < para.PickOutCnt)
                        throw new Exception($"单号为{pickListCode}的领料单中，领料项唯一码为{pickItemUniqueCode}的领用项只需要领用{pickItem.CountToPick - pickItem.PickedCount}，实际却准备领{para.PickOutCnt}");

                    pickList.Pick(pickItemUniqueCode, stock.Id, para.PickOutCnt, para.OperatorName);

                    if (pickList.Status == PickOrderStatus.Finished)
                        await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);
                    else
                        await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "领用成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> CreateNoPlanPickListAsync(NoPlanPickOutCreateDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(para.PickerName))
                        throw new Exception("未指定领料人");

                    var pickers = await _eprpPickManRepository.GetPickManNamesAsync(para.PickerName).ConfigureAwait(false);
                    if (pickers == null || pickers.Count == 0)
                        throw new Exception($"领料人{para.PickerName}未定义.");

                    var picker = pickers.FirstOrDefault(o => o.MAN_NAME == para.PickerName);
                    if (picker == null)
                        throw new Exception($"领料人{para.PickerName}未定义。");

                    var material = await _materialRepository.FindByMaterialCodeAsync(para.MaterialCode).ConfigureAwait(false);
                    if (material == null)
                        throw new Exception($"编号为{para.MaterialCode}的物料信息不存在");

                    var dpt = await _departmentRepository.FindByIdAsync(para.DepartmentId, false).ConfigureAwait(false);
                    if (dpt == null)
                        throw new Exception($"Id为{para.DepartmentId}的部门信息不存在");

                    var pickList = await _pickListManager.CreatePickList(
                        $"OU{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                        DateTime.Now,
                        para.PickType,
                        new PickerInfoOfPickList(dpt.DepartmentCode, dpt.DepartmentName, null, null, para.PickerName),
                        null,
                        new GoodsInfoOfPickList(null, null, null));

                    await _pickListManager.AddPickItem(
                        pickList,
                        DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        material.MaterialCode,
                        material.MaterialName,
                        material.Specs,
                        material.Unit,
                        para.PickCount,"");

                    await _pickListRepository.InsertAsync(pickList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "创建无计划领料成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> DeleteNoPlanPickListAsync(NoPlanPickListDelDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var pickList = await _pickListRepository.FindByPickListIdAsync(para.PickListId).ConfigureAwait(false);

                    if (pickList == null)
                        return new ResponseDto() { success = true, message = $"Id为{para.PickListId}的无计划领用单原本就不存在，默认删除成功" };

                    if (pickList.Status != PickOrderStatus.Created)
                        throw new Exception($"Id为{para.PickListId}的无计划领用单已经在执行，无法删除");

                    await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "删除无计划领用单成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ResponseDto> EditNoPlanPickListAsync(NoPlanPickListEditDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var pickList = await _pickListRepository.FindByPickListIdAsync(para.NoPlanPickListIdToEdit).ConfigureAwait(false);

                    if (pickList == null)
                        throw new Exception($"Id为{para.NoPlanPickListIdToEdit}的无计划领用单不存在，修改失败");

                    if (pickList.Status != PickOrderStatus.Created)
                        throw new Exception($"Id为{para.NoPlanPickListIdToEdit}的无计划领用单已经在执行，无法修改");

                    var pickItem = pickList.GetPickItemByUniqueCode(para.UniqueCodeToEdit);
                    if (pickItem == null)
                        throw new Exception($"Id为{para.NoPlanPickListIdToEdit}的无计划领用单不存在唯一码为{para.UniqueCodeToEdit}的领用项，无法修改");

                    if (pickItem.Status != PickItemStatus.Created)
                        throw new Exception($"Id为{para.NoPlanPickListIdToEdit}的无计划领用单中唯一码为{para.UniqueCodeToEdit}的领用项已经在执行，无法修改");


                    if (string.IsNullOrWhiteSpace(para.NewPickerName))
                        throw new Exception("未指定领料人");

                    var pickers = await _eprpPickManRepository.GetPickManNamesAsync(para.NewPickerName).ConfigureAwait(false);
                    if (pickers == null || pickers.Count == 0)
                        throw new Exception($"领料人{para.NewPickerName}未定义.");

                    var picker = pickers.FirstOrDefault(o => o.MAN_NAME == para.NewPickerName);
                    if (picker == null)
                        throw new Exception($"领料人{para.NewPickerName}未定义。");

                    pickList.ModifyPickList(
                        pickList.PickListDate, 
                        para.NewPickType, 
                        new PickerInfoOfPickList(
                            pickList.Picker.DeptCode, pickList.Picker.DeptName, 
                            pickList.Picker.GysCode, pickList.Picker.GysName, 
                            para.NewPickerName), 
                        pickList.PickBatch, 
                        pickList.Goods);
                    pickList.ModifyPickItem(
                        para.UniqueCodeToEdit,
                        pickItem.MaterialCode,
                        pickItem.MaterialName, 
                        pickItem.Specs, 
                        pickItem.Unit, 
                        para.NewPickCount);

                    await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "更新无计划领用单成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<PagedResultDto<PickItemDto>> GetPagedNoPlanPickListAsync(PagedNoPlanPickItemsQueryDto para)
        {
            try
            {
                if (para.PageIndex <= 0)
                    throw new Exception("分页号无效，必须大于0");

                if (para.PageSize <= 0)
                    throw new Exception("每页数量无效，必须大于0");

                List<PickList> unFinishedPickLists = await _pickListRepository.GetAllUnFinishedPickListsAsync(false).ConfigureAwait(false);

                List<PickList> pickLists = new List<PickList>();
                if (para.DepartmentId != null)
                //{
                //pickLists = unFinishedPickLists.Where(o => o.Picker.DeptCode == null).ToList();
                //pickLists = await _pickListRepository.GetPickListsByDepartmentCodeAsync(
                //    null,
                //    false)
                //    .ConfigureAwait(false);
                //}
                //else
                {
                    var department = await _departmentRepository.FindByIdAsync(para.DepartmentId.Value, false).ConfigureAwait(false);
                    if (department == null)
                        throw new Exception($"Id为{para.DepartmentId}的部门信息不存在");

                    pickLists = unFinishedPickLists.Where(o => o.Picker.DeptCode == department.DepartmentCode).ToList();
                    //pickLists = await _pickListRepository.GetPickListsByDepartmentCodeAsync(
                    //    department.DepartmentCode,
                    //    false)
                    //    .ConfigureAwait(false);
                }
                else
                    pickLists = unFinishedPickLists;

                if (pickLists == null || pickLists.Count == 0)
                    return new PagedResultDto<PickItemDto>() { TotalCount = 0, Items = new List<PickItemDto>() };

                var noPlanTypes = PickTypeHelper.GetNoPlanPickTypes();
                if (noPlanTypes != null && noPlanTypes.Count > 0)
                {
                    List<int> noPlanTypeNos = new List<int>();
                    foreach (var type in noPlanTypes)
                    {
                        noPlanTypeNos.Add(type.PickTypeNo);
                    }
                    pickLists = pickLists.Where(o => noPlanTypeNos.Contains(o.Type)).ToList();
                }

                if (pickLists == null || pickLists.Count == 0)
                    return new PagedResultDto<PickItemDto>() { TotalCount = 0, Items = new List<PickItemDto>() };


                var items = new List<PickItemDto>();
                foreach (var pickList in pickLists)
                {
                    foreach (var item in pickList.PickItems)
                    {
                        if (item.Status == PickItemStatus.Created || item.Status == PickItemStatus.Picking)
                        {
                            PickItemDto itemDto = new PickItemDto()
                            {
                                PickListId = pickList.Id,
                                PickItemId = item.Id,
                                PickListCode = pickList.PickListCode,
                                PickListDate = pickList.PickListDate.ToString("yyyy-MM-dd"),
                                PickType = PickTypeHelper.PickTypeToChinese(pickList.Type),
                                PickTypeNo = pickList.Type,
                                DeptCode = pickList.Picker.DeptCode,
                                DeptName = pickList.Picker.DeptName,
                                GysCode = pickList.Picker.GysCode,
                                GysName = pickList.Picker.GysName,
                                PickManName = pickList.Picker.PickManName,
                                PickBatch = pickList.PickBatch,
                                GoodsCode = pickList.Goods.GoodsCode,
                                GoodsName = pickList.Goods.GoodsName,
                                GoodsSpecs = pickList.Goods.GoodsSpecs,
                                UniqueCode = item.UniqueCode,
                                MaterialCode = item.MaterialCode,
                                MaterialName = item.MaterialName,
                                Specs = item.Specs,
                                Unit = item.Unit,
                                CountToPick = item.CountToPick,
                                PickedCount = item.PickedCount,
                                PickItemStatus = item.Status.ToString(),
                                CellCode = null, // stock?.CellData.CellCode,
                                CountInCell = null //stock?.TotalCountInTime
                            };
                            items.Add(itemDto);
                        }
                    }
                }

                items = items
                    .Where(o =>
                    (para.MaterialCodeTip == null ? true : o.MaterialCode.Contains(para.MaterialCodeTip)) &&
                    (para.MaterialNameTip == null ? true : o.MaterialName.Contains(para.MaterialNameTip)) &&
                    (para.MaterialSpecsTip == null ? true : o.Specs.Contains(para.MaterialSpecsTip)) &&
                    (para.PickerName == null ? true : o.PickManName == para.PickerName) &&
                    (para.PickTypeNo == null ? true : o.PickTypeNo == para.PickTypeNo.Value))
                    .ToList();

                if (para.SkipCount >= items.Count)    //para.SkipCount大于等于0
                    return new PagedResultDto<PickItemDto>() { TotalCount = items.Count, Items = new List<PickItemDto>() };
                else
                {
                    List<PickItemDto> result = items.GetRange(
                        para.SkipCount,
                        items.Count - para.SkipCount >= para.PageSize ? para.PageSize : items.Count - para.SkipCount);

                    //foreach (var item in result)
                    //{
                    //var stock = await _stockRepository.FindEarliestStockWithMaterialCodeAsync(item.MaterialCode, false);
                    //item.CellCode = stock?.CellData.CellCode;
                    //item.CountInCell = stock?.TotalCountInTime;
                    //}

                    return new PagedResultDto<PickItemDto> { TotalCount = items.Count, Items = result };
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public List<NoPlanPickTypeDto> GetAllNoPlanPickTypes()
        {
            List<PickTypeMap> maps = PickTypeHelper.GetNoPlanPickTypes();
            if (maps == null || maps.Count == 0)
                return new List<NoPlanPickTypeDto>();

            List<NoPlanPickTypeDto> result = new List<NoPlanPickTypeDto>();
            foreach(var map in maps)
            {
                result.Add(new NoPlanPickTypeDto() { PickTypeNo = map.PickTypeNo, PickTypeName = map.PickTypeName });
            }
            return result;
        }


        private List<Stock> SortStock(List<Stock> stocksToSort)
        {
            List<Stock> stocksRecheckedWithCheckDate = new List<Stock>();
            List<Stock> stocksRecheckedWithOutCheckDate = new List<Stock>();
            List<Stock> stocksUnRecheckedWithCheckDate = new List<Stock>();
            List<Stock> stocksUnRecheckedWithOutCheckDate = new List<Stock>();

            foreach (var stock in stocksToSort)
            {
                if (stock.CheckData.CheckType == EnumCheckType.ReCheck && stock.CheckData.CheckDate != null) 
                    stocksRecheckedWithCheckDate.Add(stock);
                else if (stock.CheckData.CheckType == EnumCheckType.ReCheck && stock.CheckData.CheckDate == null)
                    stocksRecheckedWithOutCheckDate.Add(stock);
                else if (stock.CheckData.CheckType != EnumCheckType.ReCheck && stock.CheckData.CheckDate != null)
                    stocksUnRecheckedWithCheckDate.Add(stock);
                else
                    stocksUnRecheckedWithOutCheckDate.Add(stock);
            }

            stocksRecheckedWithCheckDate = stocksRecheckedWithCheckDate
                .OrderBy(o => o.CheckData.CheckDate)
                .ThenBy(o => o.CheckData.CheckNo)
                .ThenBy(o => o.Barcode)
                .ToList();
            stocksUnRecheckedWithCheckDate = stocksUnRecheckedWithCheckDate
                .OrderBy(o => o.CheckData.CheckDate)
                .ThenBy(o => o.CheckData.CheckNo)
                .ThenBy(o => o.Barcode)
                .ToList();
            stocksRecheckedWithOutCheckDate = stocksRecheckedWithOutCheckDate.OrderBy(o => o.StockInDate).ToList();
            stocksUnRecheckedWithOutCheckDate = stocksUnRecheckedWithOutCheckDate.OrderBy(o => o.StockInDate).ToList();

            //复检的物料先用掉
            List<Stock> result = new List<Stock>();
            result.AddRange(stocksRecheckedWithCheckDate);
            result.AddRange(stocksRecheckedWithOutCheckDate);
            result.AddRange(stocksUnRecheckedWithCheckDate);
            result.AddRange(stocksUnRecheckedWithOutCheckDate);

            return result;
        }

        private async Task<AgvTask> SetAsExecutingAsync(Cell startCell, Cell endCell, string skipCode, Box box, ManageType type,string picklist,string unique)
        {
            startCell.SetSelected();
            await _cellRepository.UpdateAsync(startCell);
            _logger.Info($"设置起始库位{startCell.CellCode}为Selected");
            endCell.SetSelected();
            await _cellRepository.UpdateAsync(endCell);
            _logger.Info($"设置目标库位{endCell.CellCode}为Selected");

            AgvTask agvtask = null;
            if (type == ManageType.CTUStockIn || type == ManageType.CTUStockOut)
            {
                agvtask = await _agvTaskManager.CreateCTUStockOutByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type, picklist, unique);
            }
            if (type == ManageType.LiftStockIn)
            {
                agvtask = await _agvTaskManager.CreateLiftStockOutByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, type);
            }
            if (type == ManageType.LiftStockOut)
            {
                string endcellcode = endCell.CellCode;
                if(endCell.CellType == CellType.Skip)
                {
                    endcellcode = endCell.CellCode2;
                }
                agvtask = await _agvTaskManager.CreateLiftStockOutByStockTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endcellcode, type);
            }
            if(type == ManageType.LiftSSXOut)
            {
                agvtask = await _agvTaskManager.CreateLiftSSXTaskAsync(box.BoxCode, box.BoxTypeName, startCell.CellCode, endCell.CellCode, skipCode, type);
            }
            //同时创建AGV任务。
            return agvtask;
        }


        public async Task<List<string>> GetErpDeptTypeDetail()
        {
            List<string> result = new List<string>();
            foreach(var dto in await _erpDeptTypeDetailRepository.ToListAsync())
            {
                result.Add(JsonConvert.SerializeObject(dto));
            }
            return result;
        }


        private async Task<decimal> BoxIsFul(Guid boxId)
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
            return isFul;
        }

        private async Task<decimal> BoxIsFulMin(Guid boxId, string materialCode, decimal count)
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
            isFul -= count / material.FullBoxCount.GetValueOrDefault();
            return isFul;
            //if (isFul <= 1)
            //    return true;
            //else
            //    return false;
        }
    }
}
