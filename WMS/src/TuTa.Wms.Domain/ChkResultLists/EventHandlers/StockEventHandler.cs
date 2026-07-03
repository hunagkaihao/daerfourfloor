using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.Stocks.Events;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.ChkResultLists.EventHandlers
{
    public class StockEventHandler
        : ILocalEventHandler<StockBindBoxAndCellEvent>,
          ITransientDependency
    {
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<StockEventHandler> _logger;

        private static readonly object _locker = new object();

        public StockEventHandler(
            IChkResultListRepository chkResultListRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<StockEventHandler> logger)
        {
            _chkResultListRepository = chkResultListRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }


        public async Task HandleEventAsync(StockBindBoxAndCellEvent eventData)
        {
            /*
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    Check.NotNullOrWhiteSpace(eventData.StockBarcode, nameof(eventData.StockBarcode));

                    var chkResultExist = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(eventData.StockBarcode, eventData.CheckType).ConfigureAwait(false);
                    if (chkResultExist == null)
                        throw new Exception($"检验类型为{eventData.CheckType}，收料码为{eventData.StockBarcode}的检验结论信息不存在");

                    chkResultExist.BindToBoxAndCell(
                        eventData.BoxId, eventData.BoxCode, eventData.BoxName,
                        eventData.CellId, eventData.CellCode, eventData.CellName,
                        eventData.AreaId, eventData.AreaCode, eventData.AreaName,
                        eventData.HouseId, eventData.HouseCode, eventData.HouseName, eventData.StockCount);

                    if (chkResultExist.Status == ChkResultListStatus.Finished)
                        await _chkResultListRepository.DeleteAsync(chkResultExist).ConfigureAwait(false);
                    else
                        await _chkResultListRepository.UpdateAsync(chkResultExist).ConfigureAwait(false);

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
            */
        }
    }
}
