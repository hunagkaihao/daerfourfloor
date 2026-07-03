using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Events;
using TuTa.Wms.Cells;
using TuTa.Wms.RecheckLists.Events;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Entities;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Stocks.EventHandlers
{
    public class BoxEventHandler
         : ILocalEventHandler<BoxBindCellEvent>, //库存被领用
           ILocalEventHandler<BoxDisBindCellEvent>, //复检领用
           ITransientDependency
    {
        private readonly IBoxRepository _boxRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<BoxEventHandler> _logger;
        private readonly LocalEventBus _localEventBus;

        private static readonly object _locker = new object();

        public BoxEventHandler(
            IBoxRepository boxRepository,
            ICellRepository cellRepository,
            IWarehouseRepository warehouseRepository,
            IStockRepository stockRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<BoxEventHandler> logger,
            LocalEventBus localEventBus)
        {
            _boxRepository = boxRepository;
            _cellRepository = cellRepository;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _localEventBus = localEventBus;
        }

        public async Task HandleEventAsync(BoxBindCellEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var stocks = await _stockRepository.GetByBoxIdAsync(eventData.BoxId).ConfigureAwait(false);
                    if (stocks == null || stocks.Count == 0)
                        return;

                    var box = await _boxRepository.FindByBoxIdAsync(eventData.BoxId).ConfigureAwait(false);
                    if (box == null)
                        throw new Exception($"Id为{eventData.BoxId}的容器不存在");

                    var cell = await _cellRepository.FindByIdAsync(eventData.CellId).ConfigureAwait(false);
                    if (cell == null)
                        throw new Exception($"Id为{eventData.CellId}的库位不存在");

                    var house = await _warehouseRepository.FindByIdAsync(eventData.WarehouseId).ConfigureAwait(false);
                    if (house == null)
                        throw new Exception($"Id为{eventData.WarehouseId}的仓库不存在");

                    WarehouseArea area = null;
                    if (eventData.WarehouseAreaId != null)
                    {
                        area = house.GetAreaByAreaId(eventData.WarehouseAreaId.Value);
                        if (area == null)
                            throw new Exception($"Id为{eventData.WarehouseId}的仓库中没有Id为{eventData.WarehouseAreaId}的库区");
                    }

                    foreach (var stock in stocks)
                    {
                        stock.BindCell(
                            eventData.CellId, cell.CellCode, cell.CellName, cell.AvailableBoxSpecsNames,cell.CellType,
                            eventData.WarehouseAreaId, area.WarehouseAreaCode, area.WarehouseAreaName,
                            eventData.WarehouseId, house.WarehouseCode, house.WarehouseName);
                        await _stockRepository.UpdateAsync(stock);
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

        public async Task HandleEventAsync(BoxDisBindCellEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var stocks = await _stockRepository.GetByBoxIdAsync(eventData.BoxId).ConfigureAwait(false);
                    if (stocks == null || stocks.Count == 0)
                        return;

                    foreach (var stock in stocks)
                    {
                        stock.DisBindCell();
                        await _stockRepository.UpdateAsync(stock);
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

