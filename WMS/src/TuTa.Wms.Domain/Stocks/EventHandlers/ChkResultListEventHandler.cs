using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.ChkResultLists.Events;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Stocks.EventHandlers
{
    public class ChkResultListEventHandler
         : ILocalEventHandler<RecheckResultGettedEvent>, //复检入库，复检结果为合格，取消冻结
           ITransientDependency
    {
        private readonly IStockRepository _stockRepository;
        private readonly StocksManager _stocksManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ChkResultListEventHandler> _logger;

        private static readonly object _locker = new object();

        public ChkResultListEventHandler(
            IStockRepository stockRepository,
            StocksManager stocksManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ChkResultListEventHandler> logger)
        {
            _stockRepository = stockRepository;
            _stocksManager = stocksManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        /// <summary>
        /// 取消冻结
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task HandleEventAsync(RecheckResultGettedEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var stocks = await _stockRepository.GetByBarcodeAsync(eventData.BarcodeOfRecheckStock).ConfigureAwait(false);
                    if (stocks == null || stocks.Count == 0)
                        return;

                    foreach (var stock in stocks)
                    {
                        _stocksManager.UpdateCheckDataAftReCheck(stock, eventData.CheckOrderCode, eventData.CheckDate,
                            eventData.CheckNo, eventData.CheckType, eventData.CheckResult, eventData.PassCnt);
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


