using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.Events;
using TuTa.Wms.Cells.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Cells.EventHandlers
{
    public class BoxEventHandler
        :
        ILocalEventHandler<BoxBindCellEvent>, //容器绑定到库位
          ILocalEventHandler<BoxDisBindCellEvent>, //容器从库位中解绑
          ITransientDependency
    {
        private readonly ICellRepository _cellRepository;
        private readonly CellManager _cellManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<BoxEventHandler> _logger;

        private static readonly object _locker = new object();

        public BoxEventHandler(
            ICellRepository cellRepository,
            CellManager cellManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<BoxEventHandler> logger)
        {
            _cellRepository = cellRepository;
            _cellManager = cellManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        /// <summary>
        /// 容器绑定库位时，产生绑定事件，被绑定库位的状态在此事件处理函数中更新
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        /// 
        public virtual async Task HandleEventAsync(BoxBindCellEvent eventData)
        {
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var cell = await _cellRepository.FindAsync(eventData.CellId).ConfigureAwait(false);
                    if (cell == null)
                        throw new Exception($"不存在CellId为{eventData.CellId}的库位，不能绑定容器");

                    // 检查是否已经绑定该容器
                    if (cell.IsBoxInThisCell(eventData.BoxId))
                        return;

                    // 创建 CellBox 关联记录
                    CellBox cellBox = new CellBox(
                        eventData.CellId,
                        eventData.BoxId,
                        eventData.BoxCode,
                        eventData.BoxName,
                        eventData.BoxTypeName,
                        eventData.SpecsName,
                        eventData.Length,
                        eventData.Width,
                        eventData.Height);

                    // 添加到库位的容器集合中
                    cell.AddBox(cellBox);

                    // 保存库位
                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
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

        public async Task HandleEventAsync(BoxDisBindCellEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var cell = await _cellRepository.FindByIdAsync(eventData.CellId).ConfigureAwait(false);
                    if (cell == null)
                        throw new Exception($"不存在CellId为{eventData.CellId}的库位，不能解绑");

                    cell.RemoveBox(eventData.BoxId);

                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
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
    }
}

