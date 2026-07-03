using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.ValueObjects;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Warehouses;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Boxes.EventHandlers
{
    public class CellEventHandler
        : ILocalEventHandler<CellCreatedEvent>, //库位被创建，创建该库位对应的虚拟容器，用于没有容器的仓库
          ITransientDependency
    {
        private readonly IBoxRepository _boxRepository;
        private readonly BoxManager _boxManager;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICellRepository _cellRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<CellEventHandler> _logger;

        private static readonly object _locker = new object();

        public CellEventHandler(
            IBoxRepository boxRepository,
            BoxManager boxManager,
            IWarehouseRepository warehouseRepository,
            ICellRepository cellRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<CellEventHandler> logger)
        {
            _boxRepository = boxRepository;
            _boxManager = boxManager;
            _warehouseRepository = warehouseRepository;
            _cellRepository = cellRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        /// <summary>
        /// 库位被创建，东方电子一期，产生库位创建事件后，创建虚拟容器，并且容器绑定到库位
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public virtual async Task HandleEventAsync(CellCreatedEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    Check.NotNullOrWhiteSpace(eventData.CellCode, nameof(eventData.CellCode));

                    var boxExist = await _boxRepository.FindByBoxCodeAsync(eventData.CellCode, false).ConfigureAwait(false);
                    if (boxExist != null)
                        return;

                    var box = await _boxManager.CreateBoxAsync(
                        eventData.CellCode,
                        $"Box_{eventData.CellCode}",
                        null,
                        new BoxSpecsValObj(null, null, null, null))
                        .ConfigureAwait(false);

                    var warehouse = await _warehouseRepository.FindByIdAsync(eventData.WarehouseId).ConfigureAwait(true);
                    if (warehouse == null)
                        throw new Exception($"Id为{eventData.WarehouseId}的仓库不存在");

                    var area = eventData.WarehouseAreaId == null ? null : warehouse.GetAreaByAreaId(eventData.WarehouseAreaId.Value);

                    box.BindCell(
                        eventData.CellId, eventData.CellCode, eventData.CellName, 
                        eventData.WarehouseId, warehouse.WarehouseCode, warehouse.WarehouseName, 
                        eventData.WarehouseAreaId, area?.WarehouseAreaCode, area?.WarehouseAreaName);

                    await _boxRepository.InsertAsync(box).ConfigureAwait(false);

                    Cell cell = await _cellRepository.FindByCellCodeAsync(eventData.CellCode);
                    cell.SetCellStatus(CellStatus.Have);
                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);

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
