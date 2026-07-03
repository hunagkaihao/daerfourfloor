using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.RecheckLists.Events;
using TuTa.Wms.Stocks.Aggregates;
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
    public class Stock_RecheckListEventHandler
           : ILocalEventHandler<ReCheckStockOutEvent>,         //复检领用
             ILocalEventHandler<FreezeStockEvent>,      //收到复检项，启动库存冻结
             ILocalEventHandler<UnFreezeStockEvent>,    //收到复检项被删除，解除库存冻结
             ITransientDependency
    {
        private readonly IStockRepository _stockRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<Stock_RecheckListEventHandler> _logger;
        private readonly LocalEventBus _localEventBus;

        private static readonly object _locker = new object();

        public Stock_RecheckListEventHandler(
            IStockRepository stockRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<Stock_RecheckListEventHandler> logger,
            LocalEventBus localEventBus)
        {
            _stockRepository = stockRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _localEventBus = localEventBus;
        }

        public async Task HandleEventAsync(ReCheckStockOutEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    //var stockExist = await _stockRepository.FindAsync(eventData.StockId).ConfigureAwait(false);
                    //if (stockExist == null)
                    //    throw new Exception($"Id为{eventData.StockId}的库存不存在");

                    //if (stockExist.Barcode != eventData.Barcode)
                    //    throw new Exception($"Id为{eventData.StockId}的库存对应的收料码不是{eventData.Barcode}");
                    Stock stockExist = eventData.Stock;

                    StockRecheckOutEvent pickOutEvent = new StockRecheckOutEvent(
                        stockExist.Barcode,
                        new MaterialInfoOfStock(stockExist.Material.MaterialCode, stockExist.Material.MaterialName, stockExist.Material.Specs, stockExist.Material.Unit, stockExist.Material.FinGoodsList),
                        new CheckInfoOfStock(stockExist.CheckData.CheckOrderCode, stockExist.CheckData.CheckDate, stockExist.CheckData.CheckNo, stockExist.CheckData.CheckNoBeforeReCheck,
                        stockExist.CheckData.CheckType, stockExist.CheckData.CheckResult, stockExist.CheckData.PassCnt),
                        new SupplierInfoOfStock(stockExist.Supplier.SupplierCode, stockExist.Supplier.SupplierName, stockExist.Supplier.SupplierBatchCode),
                        null, null, null, null,
                        null, null, null,
                        new BoxInfoOfStock(stockExist.BoxData.BoxId, stockExist.BoxData.BoxCode, stockExist.BoxData.BoxName , stockExist.BoxData.FullRate),
                        new CellInfoOfStock(stockExist.CellData.CellId, stockExist.CellData.CellCode, stockExist.CellData.CellName,stockExist.CellData.AvaBoxType, stockExist.CellData.CellType),
                        new WarehouseInfoOfStock(stockExist.Warehouse.HouseId, stockExist.Warehouse.HouseCode, stockExist.Warehouse.HouseName,
                        stockExist.Warehouse.AreaId, stockExist.Warehouse.AreaCode, stockExist.Warehouse.AreaName),
                        "复检出库", 0, null, null, eventData.PickedCount, DateTime.Now, eventData.OperatorName);

                    await _localEventBus.PublishAsync(pickOutEvent);  //通知出库历史增加复检记录

                    stockExist.Remove(eventData.PickedCount);

                    if (stockExist.TotalCountInTime > 0)
                        await _stockRepository.UpdateAsync(stockExist).ConfigureAwait(false);
                    else
                        await _stockRepository.DeleteAsync(stockExist).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(FreezeStockEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var stocks = await _stockRepository.GetByCheckNoAsync(eventData.CheckNo).ConfigureAwait(false);
                    if (stocks == null || stocks.Count == 0)
                        return;

                    foreach (var stock in stocks)
                    {
                        stock.FreezeStock();
                    }

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(UnFreezeStockEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var stocks = await _stockRepository.GetByCheckNoAsync(eventData.CheckNo).ConfigureAwait(false);
                    if (stocks == null || stocks.Count == 0)
                        return;

                    foreach (var stock in stocks)
                    {
                        stock.ReturnToAvailable();
                    }

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }
    }
}



