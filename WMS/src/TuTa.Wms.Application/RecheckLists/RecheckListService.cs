using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.RecheckLists.Dtos;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using Volo.Abp;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.RecheckLists
{
    public class RecheckListService : WmsAppService, IRecheckListService
    {
        private readonly IRecheckListRepository _recheckListRepository;
        private readonly RecheckListManager _recheckListManager;
        private readonly IStockRepository _stockRepository;
        private readonly IWarehouseRepository _wareHouseRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly ILogger<RecheckListService> _logger;

        public RecheckListService(
            IRecheckListRepository recheckListRepository,
            RecheckListManager recheckListManager,
            IStockRepository stockRepository,
            IWarehouseRepository warehouseRepository,
            IBarcodeListRepository barcodeListRepository,
            ICellRepository cellRepository,
            IBoxRepository boxRepository,
            ILogger<RecheckListService> logger)
        {
            _recheckListRepository = recheckListRepository;
            _recheckListManager = recheckListManager;
            _stockRepository = stockRepository;
            _wareHouseRepository = warehouseRepository;
            _cellRepository = cellRepository;
            _barcodeListRepository = barcodeListRepository;
            _boxRepository = boxRepository;
            _logger = logger;
        }

        public async Task<int> GetUnFinishedRecheckItemsCountAsync()
        {
            try
            {
                var recheckLists = await _recheckListRepository.GetAllRecheckListsAsync(false);
                if (recheckLists == null || recheckLists.Count == 0)
                    return 0;

                int count = 0;
                foreach(var list in recheckLists)
                {
                    if (list.RecheckItems == null || list.RecheckItems.Count == 0)
                        continue;

                    foreach(var item in list.RecheckItems)
                    {
                        if (item.Status != RecheckItemStatus.Finished)
                            count++;
                    }
                }

                return count;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<RecheckItemDto>> GetUnFinishedRecheckItemsAsync(RecheckItemQueryDto para)
        {
            try
            {
                var recheckLists = await _recheckListRepository.GetAllRecheckListsAsync(false);
                if (recheckLists == null || recheckLists.Count == 0)
                    return new List<RecheckItemDto>();

                List<RecheckItemDto> itemDtos = new List<RecheckItemDto>();
                foreach (var list in recheckLists)
                {
                    if (list.RecheckItems == null || list.RecheckItems.Count == 0)
                        continue;

                    foreach(var item in list.RecheckItems)
                    {
                        if (item.Status != RecheckItemStatus.Finished)
                        {
                            RecheckItemDto itemDto = new RecheckItemDto()
                            {
                                RecheckListCode = list.RecheckListCode,
                                RecheckListDate = list.RecheckListDate.ToString("yyyy-MM-dd"),
                                CheckNo = item.CheckNo,
                                Barcode = item.Barcode,
                                MaterialCode = item.Material.MaterialCode,
                                MaterialName = item.Material.MaterialName,
                                MaterialSpecs = item.Material.MaterialSpecs,
                                Unit = item.Material.Unit,
                                ExpiryDays = item.Material.ExpiryDays,
                                ExpiryLimitDate = item.ExpiryLimitDate,
                                RecheckTimes = item.RecheckTimes,
                                CheckCount = item.CheckCount,
                                PickedCount = item.CheckCount,
                                //PickedCount = item.PickedCount,
                                Status = item.Status.ToString()
                            };
                            itemDtos.Add(itemDto);
                        }
                    }
                }

                //return itemDtos.Where(o => 
                //    (para.CheckNoTip == null ? true : o.CheckNo.Contains(para.CheckNoTip)) &&
                //    ((para.MaterialCode == null ? true : o.MaterialName.Contains(para.MaterialCode)) ||
                //    (para.MaterialCode == null ? true : o.MaterialSpecs.Contains(para.MaterialCode))))
                //    .OrderBy(o => o.RecheckListDate)
                //    .ToList();
                return itemDtos.Where(o =>
                    (para.QueryBy == 1 ? (para.MaterialCode == null ? true : o.MaterialCode.Contains(para.MaterialCode)) : true) &&
                    (para.QueryBy == 2 ? (para.MaterialNameTip == null ? true : o.MaterialName.Contains(para.MaterialNameTip)) : true) &&
                    (para.QueryBy == 3 ? (para.MaterialSpecsTip == null ? true : o.MaterialSpecs.Contains(para.MaterialSpecsTip)) : true) &&
                    (para.QueryBy == 4 ? (para.CheckNoTip == null ? true : o.CheckNo.Contains(para.CheckNoTip)) : true)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<RecheckStockDto>> GetRecheckStocksAsync(string recheckListCode, string barcode)
        {
            try
            {
                var recheckList = await _recheckListRepository.FindByReCheckListCodeAsync(recheckListCode).ConfigureAwait(false);
                if (recheckList == null)
                    //throw new Exception($"单号为{recheckListCode}的复检单不存在");
                    return new List<RecheckStockDto>();

                if (recheckList.RecheckItems == null || recheckList.RecheckItems.Count == 0)
                    //throw new Exception($"单号为{recheckListCode}的复检单没有复检项");
                    return new List<RecheckStockDto>();


                var stocks = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
                if (stocks == null || stocks.Count == 0)
                    throw new Exception($"收料码为{barcode}的库存不存在");


                var recheckItem = recheckList.GetReCheckItemByBarcode(stocks[0].CheckData.CheckNo);
                if (recheckItem == null)
                    //throw new Exception($"单号为{recheckListCode}的复检单没有收料码为{barcode}的复检项");
                    return new List<RecheckStockDto>();

                recheckItem.RemoveRecheckStocks();

                decimal sampleCount = recheckItem.CheckCount - recheckItem.PickedCount;
                if (sampleCount <= 0)
                    //throw new Exception($"复检单{recheckListCode}中收料码为{barcode}的复检项的抽检数量{sampleCount}无效");
                    return new List<RecheckStockDto>();

                List<RecheckStockDto> result = new List<RecheckStockDto>();

                foreach (var stock in stocks)
                {
                    Warehouse warehouse = null;
                    WarehouseArea warehouseArea = null;
                    if (stock.Warehouse.HouseId != null)
                    {
                        warehouse = await _wareHouseRepository.FindByIdAsync(stock.Warehouse.HouseId.Value).ConfigureAwait(false);
                        if (stock.Warehouse.AreaId != null)
                            warehouseArea = warehouse.GetAreaByAreaId(stock.Warehouse.AreaId.Value);
                    }

                    Cell cell = null;
                    if (stock.CellData.CellId != null)
                        cell = await _cellRepository.FindAsync(stock.CellData.CellId.Value).ConfigureAwait(false);

                    Box box = null;
                    if (stock.BoxData.BoxId != null)
                        box = await _boxRepository.FindByBoxIdAsync(stock.BoxData.BoxId.Value).ConfigureAwait(false);

                    if (stock.TotalCountInTime >= sampleCount)
                    {
                        RecheckStockDto stockDto = new RecheckStockDto()
                        {
                            RecheckListCode = recheckListCode,
                            Barcode = barcode,
                            MaterialCode = stock.Material.MaterialCode,
                            MaterialName = stock.Material.MaterialName,
                            MaterialSpecs = stock.Material.Specs,
                            MaterialUnit = stock.Material.Unit,
                            WarehouseName = warehouse == null ? null : warehouse.WarehouseName,
                            WarehouseAreaName = warehouseArea == null ? null : warehouseArea.WarehouseAreaName,
                            CellCode = cell == null ? null : cell.CellCode,
                            BoxCode = box == null ? null : box.BoxCode,
                            OldCheckNo = stock.CheckData.CheckNo,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            StockCount = stock.TotalCountInTime,
                            PickCount = sampleCount
                        };
                        sampleCount = sampleCount - sampleCount;
                        result.Add(stockDto);
                        recheckItem.AddRecheckStock(stock.Id, stockDto.PickCount);
                        break;
                    }
                    else
                    {
                        RecheckStockDto stockDto = new RecheckStockDto()
                        {
                            RecheckListCode = recheckListCode,
                            Barcode = barcode,
                            MaterialCode = stock.Material.MaterialCode,
                            MaterialName = stock.Material.MaterialName,
                            MaterialSpecs = stock.Material.Specs,
                            MaterialUnit = stock.Material.Unit,
                            WarehouseName = warehouse == null ? null : warehouse.WarehouseName,
                            WarehouseAreaName = warehouseArea == null ? null : warehouseArea.WarehouseAreaName,
                            CellCode = cell == null ? null : cell.CellCode,
                            BoxCode = box == null ? null : box.BoxCode,
                            OldCheckNo = stock.CheckData.CheckNo,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            StockCount = stock.TotalCountInTime,
                            PickCount = stock.TotalCountInTime
                        };
                        sampleCount = sampleCount - stock.TotalCountInTime;
                        result.Add(stockDto);
                        recheckItem.AddRecheckStock(stock.Id, stock.TotalCountInTime);
                    }

                }

                await _recheckListRepository.UpdateAsync(recheckList).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> RecheckStockPickOutAsync(RecheckPickOutDto para)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var recheckList = await _recheckListRepository.FindByReCheckListCodeAsync(para.RecheckListCode).ConfigureAwait(false);
                    if (recheckList == null)
                        throw new Exception($"单号为{para.RecheckListCode}的复检单不存在");

                    if (recheckList.RecheckItems == null || recheckList.RecheckItems.Count == 0)
                        throw new Exception($"单号为{para.RecheckListCode}的复检单没有复检项");


                    var recheckItem = recheckList.GetReCheckItemByBarcode(para.Barcode); //返回引用
                    if (recheckItem == null)
                        throw new Exception($"单号为{para.RecheckListCode}的复检单没有收料码为{para.Barcode}的复检项");

                    var box = await _boxRepository.FindByBoxCodeAsync(para.BoxCode).ConfigureAwait(false);
                    if (box == null)
                        throw new Exception($"容器码为{para.BoxCode}的容器不存在");

                    var stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, para.Barcode).ConfigureAwait(false);
                    if (stock == null)
                        throw new Exception($"容器{para.BoxCode}中不存在收料码为{para.Barcode}的库存");

                    var recheckStock = recheckItem.GetRecheckStockByStockId(stock.Id);

                    if (recheckStock == null)
                        throw new Exception($"单号为{para.RecheckListCode}的复检单中，针对收料码为{para.Barcode}的复检项，没有指定从容器{para.BoxCode}中抽检");

                    if (para.PickOutCnt < 0 || para.PickOutCnt > recheckItem.CheckCount)
                        throw new Exception($"复检待出库数量{para.PickOutCnt}必须大于0，并小于抽检数量{recheckItem.CheckCount}");

                    recheckList.PickAway(para.Barcode, stock.Id, para.PickOutCnt, para.OperatorName);

                    if (recheckList.Status == RecheckListStatus.Created)
                        throw new Exception("复检出库后，复检单状态仍为Created");
                    if (recheckList.Status == RecheckListStatus.Picking)
                        await _recheckListRepository.UpdateAsync(recheckList).ConfigureAwait(false);
                    else
                        await _recheckListRepository.DeleteAsync(recheckList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "复检领料成功" };
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
