using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.Stocks.Events;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.StockInHistories.EventHandlers
{
    public class StockInHistory_StockEventHandler
         : ILocalEventHandler<StockBindBoxAndCellEvent>, //物料人工入库
           ITransientDependency
    {
        private readonly IStockInHistoryRepository _stockHistoryRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<StockInHistory_StockEventHandler> _logger;
        private readonly LocalEventBus _localEventBus;

        private static readonly object _locker = new object();

        public StockInHistory_StockEventHandler(
            IStockInHistoryRepository stockHistoryRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<StockInHistory_StockEventHandler> logger,
            LocalEventBus localEventBus)
        {
            _stockHistoryRepository = stockHistoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _localEventBus = localEventBus;
        }

        public async Task HandleEventAsync(StockBindBoxAndCellEvent eventData)
        {
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    StockInHistory stockHistory = new StockInHistory(
                        eventData.StockBarcode,
                        eventData.MaterialCode,
                        eventData.MaterialName,
                        eventData.Specs,
                        eventData.Unit,
                        eventData.HouseCode,
                        eventData.HouseName,
                        eventData.AreaCode,
                        eventData.AreaName,
                        eventData.CellCode,
                        eventData.CellName,
                        eventData.BoxCode,
                        eventData.BoxName,
                        eventData.StockInType,
                        eventData.StockCount,
                        eventData.StockInDate,
                        operatorName: eventData.Operator,
                        batchNo: eventData.BatchCode);

                    await _stockHistoryRepository.InsertAsync(stockHistory).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
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
