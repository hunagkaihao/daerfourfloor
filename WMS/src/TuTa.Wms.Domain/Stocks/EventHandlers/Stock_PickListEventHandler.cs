using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.PickLists.Events;
using TuTa.Wms.Stocks.Events;
using TuTa.Wms.Stocks.ValueObjects;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Stocks.EventHandlers
{
    public class PickListEventHandler
         : ILocalEventHandler<PickListStockOutEvent>, //库存被领用
           ITransientDependency
    {
        private readonly IStockRepository _stockRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<PickListEventHandler> _logger;
        private readonly LocalEventBus _localEventBus;

        private static readonly object _locker = new object();

        public PickListEventHandler(
            IStockRepository stockRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<PickListEventHandler> logger,
            LocalEventBus localEventBus)
        {
            _stockRepository = stockRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _localEventBus = localEventBus;
        }

        public async Task HandleEventAsync(PickListStockOutEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                //try
                //{
                //    if (eventData == null)
                //        throw new ArgumentNullException(nameof(eventData));

                //    var stockExist = await _stockRepository.FindAsync(eventData.StockId).ConfigureAwait(false);
                //    if (stockExist == null)
                //        throw new Exception($"Id为{eventData.StockId}的库存不存在");

                //    StockPickOutEvent pickOutEvent = new StockPickOutEvent(
                //        stockExist.Barcode,
                //        new MaterialInfoOfStock(stockExist.Material.MaterialCode, stockExist.Material.MaterialName, stockExist.Material.Specs, stockExist.Material.Unit),
                //        new CheckInfoOfStock(stockExist.CheckData.CheckOrderCode, stockExist.CheckData.CheckDate, stockExist.CheckData.CheckNo, stockExist.CheckData.CheckNoBeforeReCheck,
                //        stockExist.CheckData.CheckType, stockExist.CheckData.CheckResult, stockExist.CheckData.PassCnt),
                //        new SupplierInfoOfStock(stockExist.Supplier.SupplierCode, stockExist.Supplier.SupplierName,stockExist.Supplier.SupplierBatchCode),
                //        eventData.DeptCode, eventData.DeptName, eventData.GysCode, eventData.GysName, eventData.PickerName, 
                //        eventData.GoodsCode, eventData.GoodsName, eventData.GoodsSpecs,
                //        new BoxInfoOfStock(stockExist.BoxData.BoxId, stockExist.BoxData.BoxCode, stockExist.BoxData.BoxName),
                //        new CellInfoOfStock(stockExist.CellData.CellId, stockExist.CellData.CellCode, stockExist.CellData.CellName),
                //        new WarehouseInfoOfStock(stockExist.Warehouse.HouseId, stockExist.Warehouse.HouseCode, stockExist.Warehouse.HouseName,
                //        stockExist.Warehouse.AreaId, stockExist.Warehouse.AreaCode, stockExist.Warehouse.AreaName),
                //        eventData.PickTypeChs, (short)eventData.PickType, eventData.PickBatch, eventData.UniqueCode, eventData.PickOutCnt, DateTime.Now,eventData.OperatorName);

                //    await _localEventBus.PublishAsync(pickOutEvent);  //通知出库历史增加正常出库记录

                //    stockExist.Remove(eventData.PickOutCnt);

                //    if (stockExist.TotalCountInTime > 0)
                //        await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                //    else
                //        await _stockRepository.DeleteAsync(stockExist).ConfigureAwait(false);

                //    await uow.SaveChangesAsync().ConfigureAwait(false);
                //    await uow.CompleteAsync().ConfigureAwait(false);
                //}
                //catch (Exception ex)
                //{
                //    _logger.Error(ex.Message);
                //    //await uow.RollbackAsync().ConfigureAwait(false);
                //    throw new UserFriendlyException(ex.Message);
                //}
            }
        }
    }
}


