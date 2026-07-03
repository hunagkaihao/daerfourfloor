using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Entities;
using TuTa.Wms.PickLists.ValueObjects;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Events;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Warehouses.Entities;

using Volo.Abp.EventBus.Local;

namespace TuTa.Wms.PickLists
{
    public class PickListManager : WmsDomainService
    {
        private readonly IPickListRepository _pickListRepository;
        private readonly LocalEventBus _localEventBus;

        public PickListManager(IPickListRepository pickOrderRepository,
            LocalEventBus localEventBus)
        {
            _pickListRepository = pickOrderRepository;
            _localEventBus = localEventBus;
        }

        public async Task<PickList> CreatePickList(
            string pickListCode,
            DateTime pickListDate,
            int type,
            PickerInfoOfPickList picker,
            string pickBatch,
            GoodsInfoOfPickList goods)
        {            
            var pickListExist = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
            if (pickListExist != null)
                throw new Exception($"单号为{pickListCode}的领料单已经存在");

            PickList pickList = new PickList(GuidGenerator.Create(), pickListCode, pickListDate, type, picker, pickBatch, goods);

            return pickList;
        }

        public async Task AddPickItem(
            PickList pickListToAdd,
            string uniqueCode,
            string materialCode,
            string materialName,
            string specs,
            string unit,
            decimal countToPick,
            string checkNo)
        {
            var pickLists = await _pickListRepository.GetAllPickListsAsync(false).ConfigureAwait(false);
            foreach (var pickList in pickLists)
            {
                foreach (var pickItem in pickList.PickItems)
                {
                    if (pickItem.UniqueCode == uniqueCode)
                        throw new Exception($"值为{uniqueCode}的UniqueCode已经存在，不能重复");
                }
            }

            pickListToAdd.AddPickItem(uniqueCode, materialCode, materialName, specs, unit, countToPick, checkNo);
        }

        public async Task<List<PickStock>> GetAllPickStocksOfMaterialAsync(string materialCode)
        {
            List<PickList> pickLists = await _pickListRepository.GetAllPickListsAsync(false).ConfigureAwait(false);
            List<PickStock> pickStocks = new List<PickStock>();
            foreach (var pickList in pickLists)
            {
                var pickItems = pickList.PickItems.Where(o => o.MaterialCode == materialCode).ToList();
                foreach(var pickItem in pickItems)
                    pickStocks.AddRange(pickItem.GetAllPickStocks());
            }
            return pickStocks;
        }

        public async Task<PickList> GetPickListByPickItemIdAsync(int pickItemId)
        {
            List<PickList> pickLists = await _pickListRepository.GetAllPickListsAsync(true).ConfigureAwait(false);

            foreach (var list in pickLists)
            {
                var pickItems = list.PickItems.Where(o => o.Id == pickItemId).ToList();
                if (pickItems != null && pickItems.Count > 0)
                {
                    return list;
                }
            }
            return null;
        }

        /// <summary>
        /// 清理过期的领料库存分配，返回清理的数量
        /// </summary>
        /// <param name="pickList"></param>
        /// <returns></returns>
        public int CleanPickListStocksWhichAreTimeOver(PickList pickList)
        {
            if (pickList.PickItems == null || pickList.PickItems.Count == 0)
                return 0;

            int ret = 0;
            foreach(var pickItem in pickList.PickItems)
            {
                if (pickItem.PickStocks == null || pickItem.PickStocks.Count == 0)
                    continue;

                List<PickStock> stocksToRemove = new List<PickStock>();
                foreach (var pickStock in pickItem.PickStocks)
                {
                    if (pickStock.OverTimeCheck())
                        stocksToRemove.Add(pickStock);
                }

                foreach (var stockToRemove in stocksToRemove)
                {
                    pickItem.RemoveOnePickStock(stockToRemove.StockId);
                    ret++;
                }
            }

            return ret;
        }

        public async void StockPickOut(Stock stock,PickList pickList,string uniqueCode,string operatorName,WarehouseArea area,decimal count)
        {

            StockPickOutEvent pickOutEvent = new StockPickOutEvent(
                stock.Barcode,
                new MaterialInfoOfStock(stock.Material.MaterialCode, stock.Material.MaterialName, stock.Material.Specs, stock.Material.Unit,stock.Material.FinGoodsList),
                new CheckInfoOfStock(stock.CheckData.CheckOrderCode, stock.CheckData.CheckDate, stock.CheckData.CheckNo, stock.CheckData.CheckNoBeforeReCheck,
                stock.CheckData.CheckType, stock.CheckData.CheckResult, stock.CheckData.PassCnt),
                new SupplierInfoOfStock(stock.Supplier.SupplierCode, stock.Supplier.SupplierName, stock.Supplier.SupplierBatchCode),
                pickList.Picker.DeptCode, pickList.Picker.DeptName, pickList.Picker.GysCode, pickList.Picker.GysName, pickList.Picker.PickManName,
                pickList.Goods.GoodsCode, pickList.Goods.GoodsName, pickList.Goods.GoodsSpecs,
                new BoxInfoOfStock(stock.BoxData.BoxId, stock.BoxData.BoxCode, stock.BoxData.BoxName,stock.BoxData.FullRate.GetValueOrDefault()),
                new CellInfoOfStock(stock.CellData.CellId, stock.CellData.CellCode, stock.CellData.CellName,stock.CellData.AvaBoxType,stock.CellData.CellType),
                new WarehouseInfoOfStock(stock.Warehouse.HouseId, stock.Warehouse.HouseCode, stock.Warehouse.HouseName,
                stock.Warehouse.AreaId, stock.Warehouse.AreaCode, stock.Warehouse.AreaName),
                PickTypeHelper.PickTypeToChinese(pickList.Type), (short)pickList.Type, pickList.PickBatch, uniqueCode, count, DateTime.Now, operatorName,
                area.WarehouseAreaCode,area.WarehouseAreaName);
            await _localEventBus.PublishAsync(pickOutEvent);
        }
    }
}
