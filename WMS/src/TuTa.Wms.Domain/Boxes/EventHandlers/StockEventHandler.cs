using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.Entities;
using TuTa.Wms.Stocks.Events;
using TuTa.Wms.Warehouses;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Boxes.EventHandlers
{
    public class StockEventHandler
        : ILocalEventHandler<StockBindBoxEvent>, //库存绑定到容器
          ILocalEventHandler<StockUsedUpEvent>,
          ITransientDependency
    {
        private readonly IBoxRepository _boxRepository;
        private readonly BoxManager _boxManager;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<StockEventHandler> _logger;

        private static readonly object _locker = new object();

        public StockEventHandler(
            IBoxRepository boxRepository,
            BoxManager boxManager,
            IWarehouseRepository warehouseRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<StockEventHandler> logger)
        {
            _boxRepository = boxRepository;
            _boxManager = boxManager;
            _warehouseRepository = warehouseRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        /// <summary>
        /// 库存绑定容器时，产生该事件，容器的状态在此事件中更新
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task HandleEventAsync(StockBindBoxEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var boxExist = await _boxRepository.FindAsync(eventData.BoxId).ConfigureAwait(false);
                    if (boxExist == null) 
                        throw new Exception($"Id为{eventData.BoxId}的容器不存在");

                    //var stockExist = await _stockRepository.FindAsync(eventData.StockId).ConfigureAwait(false);
                    //if (stockExist == null)
                    //    throw new Exception($"Id为{eventData.StockId}的库存不存在");

                    boxExist.AddStock(new BoxStock(eventData.BoxId, eventData.StockId, eventData.StockBarcode));
                    await _boxRepository.UpdateAsync(boxExist).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    await uow.RollbackAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task HandleEventAsync(StockUsedUpEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    if (eventData.BoxId == null) //会不会存在没有在容器中的物料被用完？？ 暂时认为可能存在
                        return;

                    var boxExist = await _boxRepository.FindByBoxIdAsync(eventData.BoxId.Value).ConfigureAwait(false);
                    if (boxExist == null)
                        throw new Exception($"Id为{eventData.BoxId.Value}的容器不存在");

                    //var stockExist = await _stockRepository.FindAsync(eventData.StockId).ConfigureAwait(false);
                    //if (stockExist == null)
                    //    throw new Exception($"Id为{eventData.StockId}的库存不存在");

                    boxExist.RemoveStock(eventData.StockId);

                    await _boxRepository.UpdateAsync(boxExist).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    await uow.RollbackAsync().ConfigureAwait(false);
                }
            }
        }

        //[UnitOfWork]
        //public virtual async Task HandleEventAsync(Cell eventData)
        //{
        //    //TODO: your code that does somthing on the event
        //    try
        //    {
        //        if (eventData == null)
        //            throw new ArgumentNullException(nameof(eventData));

        //        Check.NotNullOrWhiteSpace(eventData.CellCode, nameof(eventData.CellCode));

        //        var boxExist = await _boxRepository.FindByBoxCodeAsync(eventData.CellCode).ConfigureAwait(false);
        //        if (boxExist != null)
        //            return;

        //        var box = await _boxManager.CreateBoxAsync(
        //            eventData.CellCode,
        //            $"Box_{eventData.CellCode}",
        //            null,
        //            new ValueObjects.BoxSpecsValObj(null, null, null, null))
        //            .ConfigureAwait(false);

        //        await _boxRepository.InsertAsync(box).ConfigureAwait(false);
        //        //uow.SaveChangesAsync().GetAwaiter().GetResult();
        //        //uow.CompleteAsync().GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message);
        //    }
        //}
    }
}
