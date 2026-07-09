using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Dtos;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Boxes.Entities;
using TuTa.Wms.Cells;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Application.Contracts.Shared;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TuTa.Wms.AgvTasks
{
    public class AgvTaskService : WmsAppService, IAgvTaskService
    {
        private ILogger<AgvTaskService> _logger;
        private AgvTaskManager _agvTaskManager;
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IRepository<BoxStock> _boxStockRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IStockRepository _stockRepository;
        private readonly RcsApiManager _rcsApiManager;
        private readonly IServiceProvider _serviceProvider;

        public AgvTaskService(
            ILogger<AgvTaskService> logger
            , AgvTaskManager agvTaskManager
            , IAgvTaskRepository agvTaskRepository
            , IBoxRepository boxRepository
            , IRepository<BoxStock> boxStockRepository
            , ICellRepository cellRepository
            , IStockRepository stockRepository
            , RcsApiManager rcsApiManager
            , IServiceProvider serviceProvider)
        {
            _logger = logger;
            _agvTaskManager = agvTaskManager;
            _agvTaskRepository = agvTaskRepository;
            _boxRepository = boxRepository;
            _boxStockRepository = boxStockRepository;
            _cellRepository = cellRepository;
            _stockRepository = stockRepository;
            _rcsApiManager = rcsApiManager;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// CTU任务回调
        /// </summary>
        /// <param name="reqCode"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<ResultAgvTaskDto> CtuCallbackAsync(AgvCallBackRequest input)
        {

            if (input.Method == "taskStart")
            {
                try
                {
                   var agvTask =  await _agvTaskManager.SetAsTaskStart(input.TaskCode);
                    return new ResultAgvTaskDto("0", "成功", input.ReqCode, "");
                }
                catch (Exception e)
                {
                    return new ResultAgvTaskDto("1", e.Message, input.ReqCode, "");
                }

            }
            else if (input.Method == "cellOut")
            {
                try
                {
                    //设置库位为可用
                    var agvTask = await _agvTaskManager.SetAsCellOut(input.TaskCode);
                    if (agvTask.StockTyp == ManageType.CTUSSXIn)
                    {
                        Console.WriteLine("预调度");
                        await _agvTaskManager.CreatePreAsync("101", "10", "11");
                    }
                    return new ResultAgvTaskDto("0", "成功", input.ReqCode, "");
                }
                catch (Exception e)
                {
                    return new ResultAgvTaskDto("1", e.Message, input.ReqCode, "");
                }

            }
            else if (input.Method == "taskFinish")
            {
                try
                {
                    var agvTask = await _agvTaskManager.SetAsCompletedAsync(input.TaskCode);
                    return new ResultAgvTaskDto("0", "成功", input.ReqCode, "");
                }
                catch (Exception e)
                {
                    return new ResultAgvTaskDto("1", e.Message, input.ReqCode, "");

                }

            }
            else if (input.Method == "taskCancel")
            {
                try
                {
                    var agvTask = await _agvTaskManager.SetAsCancelAsync(input.TaskCode);
                    return new ResultAgvTaskDto("0", "成功", input.ReqCode, "");
                }
                catch (Exception e)
                {
                    return new ResultAgvTaskDto("1", e.Message, input.ReqCode, "");
                }
            }
            else
            {
                return new ResultAgvTaskDto("0", "成功", input.ReqCode, "");
            }
        }
        /// <summary>
        /// 分页获取AGV任务列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        public async Task<AgvTaskPagedResultDto> GetPagedListAsync(AgvTaskPagedQueryDto input)
        {
            try
            {
                // 使用仓储的高效分页查询方法，避免加载全部数据到内存
                var (items, totalCount) = await _agvTaskRepository.GetPagedListAsync(input);

                // 将实体转换为DTO
                var itemDtos = items.Select(x => new AgvTaskDto
                {
                    Id = x.Id,
                    ReqCode = x.ReqCode,
                    ReqTime = x.ReqTime,
                    ClientCode = x.ClientCode,
                    TokenCode = x.TokenCode,
                    TaskTyp = x.TaskTyp,
                    StockTyp = x.StockTyp,
                    WbCode = x.WbCode,
                    PodCode = x.PodCode,
                    PodDir = x.PodDir,
                    PodTyp = x.PodTyp,
                    MaterialLot = x.MaterialLot,
                    Priority = x.Priority,
                    TaskCode = x.TaskCode,
                    AgvCode = x.AgvCode,
                    Data = x.Data,
                    UserCallCodePath = x.UserCallCodePath,
                    RefTask = x.RefTask,
                    AgvTaskStatus = x.AgvTaskStatus,
                    BoxCode = x.BoxCode,
                    CtnrTyp = x.CtnrTyp,
                    StartPositionCode = x.StartPositionCode,
                    EndPositionCode = x.EndPositionCode,
                    PickListCode = x.PickListCode,
                    UniqueCode = x.UniqueCode,
                    CreationTime = x.CreationTime,
                    LastModificationTime = x.LastModificationTime,
                    CreatorId = x.CreatorId,
                    LastModifierId = x.LastModifierId,
                    TaskStartTime = x.TaskStartTime
                }).ToList();

                return new AgvTaskPagedResultDto
                {
                    TotalCount = totalCount,
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    Items = itemDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分页获取AGV任务列表时发生异常");
                throw;
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> CancelAgvTaskAsync(int taskId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    _logger.LogInformation($"=== 开始取消AGV任务 taskId={taskId} ===");

                    var task = await _agvTaskRepository.FindAsync(taskId).ConfigureAwait(false);
                    if (task == null)
                    {
                        _logger.LogWarning($"取消任务失败：任务{taskId}不存在");
                        return new ResponseDto() { success = false, message = $"任务{taskId}不存在" };
                    }

                    _logger.LogInformation($"任务信息: ReqCode={task.ReqCode}, TaskCode={task.TaskCode}, BoxCode={task.BoxCode}, 状态={task.AgvTaskStatus}, 起点={task.StartPositionCode}, 终点={task.EndPositionCode}");

                    // 1. 下发给RCS取消任务
                    _logger.LogInformation("步骤1: 下发给RCS取消任务");
                    var rcsResult = await _rcsApiManager.CancelTaskAsync(task.ReqCode, task.TaskCode).ConfigureAwait(false);
                    if (rcsResult.Code != "0")
                        _logger.LogWarning($"RCS取消任务返回非0: Code={rcsResult.Code}, Message={rcsResult.Message}");
                    else
                        _logger.LogInformation("RCS取消任务成功");

                    // 2. 标记任务状态为取消
                    _logger.LogInformation("步骤2: 标记任务状态为取消");
                    task.SetAsCancel();
                    await _agvTaskRepository.UpdateAsync(task).ConfigureAwait(false);

                    // 3. 根据BoxCode处理容器解绑、库位恢复、库存删除
                    _logger.LogInformation($"步骤3: 处理容器={task.BoxCode}的相关操作");
                    if (!string.IsNullOrEmpty(task.BoxCode))
                    {
                        var box = await _boxRepository.FindByBoxCodeAsync(task.BoxCode).ConfigureAwait(false);
                        if (box != null)
                        {
                            _logger.LogInformation($"找到容器: BoxCode={box.BoxCode}, CellData.CellCode={box.CellData?.CellCode}, Status={box.Status}");

                            // 解绑库位（如果有）
                            Guid? cellId = box.CellData.CellId;
                            if (cellId.HasValue)
                            {
                                _logger.LogInformation($"步骤3.1: 恢复库位CellId={cellId}状态为Nohave");
                                var cell = await _cellRepository.FindByIdAsync(cellId.Value).ConfigureAwait(false);
                                if (cell != null)
                                {
                                    cell.SetCellStatus(CellStatus.Nohave);
                                    cell.SetEnable();
                                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
                                    _logger.LogInformation($"库位{cell.CellCode}状态已重置为Nohave");
                                }
                                else
                                    _logger.LogWarning($"库位CellId={cellId}不存在，跳过库位恢复");
                            }
                            else
                                _logger.LogInformation("容器未绑定库位，跳过库位恢复");

                            // 恢复终点库位锁定状态
                            if (!string.IsNullOrEmpty(task.EndPositionCode))
                            {
                                _logger.LogInformation($"步骤3.1.1: 恢复终点库位{task.EndPositionCode}状态");
                                var endCell = await _cellRepository.FindByCellCodeAsync(task.EndPositionCode).ConfigureAwait(false);
                                if (endCell != null && endCell.RunStatus == CellRunStatus.Selected)
                                {
                                    endCell.SetEnable();
                                    await _cellRepository.UpdateAsync(endCell).ConfigureAwait(false);
                                    _logger.LogInformation($"终点库位{endCell.CellCode}已恢复为Enable");
                                }
                                else
                                    _logger.LogInformation($"终点库位{task.EndPositionCode}不存在或未锁定，跳过");
                            }

                            // 删除BoxStock中间表记录
                            _logger.LogInformation("步骤3.2: 删除BoxStock中间表记录");
                            await _boxStockRepository.DeleteAsync(bs => bs.BoxId == box.Id).ConfigureAwait(false);

                            // 获取并删除该料箱下的所有库存
                            var stocks = await _stockRepository.GetByBoxIdAsync(box.Id).ConfigureAwait(false);
                            if (stocks != null && stocks.Count > 0)
                            {
                                _logger.LogInformation($"步骤3.3: 删除料箱下的{stocks.Count}条库存");
                                foreach (var stock in stocks)
                                {
                                    _logger.LogInformation($"  删除库存: StockId={stock.Id}, Barcode={stock.Barcode}, 物料={stock.Material?.MaterialCode}, 数量={stock.TotalCountInTime}");
                                    box.RemoveStock(stock.Id);
                                    await _stockRepository.DeleteAsync(stock).ConfigureAwait(false);
                                }
                            }
                            else
                                _logger.LogInformation("料箱下无库存，跳过删除");

                            // 通知RCS解绑容器与库位
                            var cellCode = box.CellData?.CellCode ?? task.StartPositionCode;
                            if (!string.IsNullOrEmpty(cellCode) && !string.IsNullOrEmpty(task.CtnrTyp))
                            {
                                _logger.LogInformation($"步骤3.4: 通知RCS解绑容器与库位, cellCode={cellCode}, ctnrTyp={task.CtnrTyp}, boxCode={task.BoxCode}");
                                try
                                {
                                    var bindResult = await _rcsApiManager.BindCtnrAndBinAsync(
                                        task.ReqCode,
                                        cellCode,
                                        task.CtnrTyp,
                                        task.BoxCode,
                                        "0").ConfigureAwait(false);
                                    _logger.LogInformation($"RCS解绑结果: Code={bindResult?.Code}, Message={bindResult?.Message}");
                                }
                                catch (Exception rcsEx)
                                {
                                    _logger.LogWarning($"RCS解绑调用异常(不影响取消): {rcsEx.Message}");
                                }
                            }
                            else
                                _logger.LogInformation($"跳过RCS解绑(cellCode={cellCode}, ctnrTyp={task.CtnrTyp})");

                            // 解绑容器(清空CellData)
                            _logger.LogInformation("步骤3.5: 解绑容器清空CellData");
                            box.DisBindCell();
                            box.SetNoHave();
                            await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
                        }
                        else
                            _logger.LogWarning($"容器{task.BoxCode}不存在，跳过容器操作");
                    }
                    else
                        _logger.LogInformation("任务无关联容器(BoxCode为空)，跳过容器操作");

                    await uow.CompleteAsync().ConfigureAwait(false);

                    _logger.LogInformation($"=== AGV任务{taskId}取消成功 ===");
                    return new ResponseDto() { success = true, message = "AGV任务取消成功，已恢复库位并删除组盘库存" };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"取消AGV任务{taskId}时发生异常");
                    return new ResponseDto() { success = false, message = $"取消任务失败: {ex.Message}" };
                }
            }
        }
    }
}
