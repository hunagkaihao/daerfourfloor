using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Dtos;
using Volo.Abp.Uow;

namespace TuTa.Wms.AgvTasks
{
    public class AgvTaskService : WmsAppService, IAgvTaskService
    {
        private ILogger<AgvTaskService> _logger;
        private AgvTaskManager _agvTaskManager;
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly IServiceProvider _serviceProvider;

        public AgvTaskService(
            ILogger<AgvTaskService> logger
            , AgvTaskManager agvTaskManager
            , IAgvTaskRepository agvTaskRepository
            , IServiceProvider serviceProvider)
        {
            _logger = logger;
            _agvTaskManager = agvTaskManager;
            _agvTaskRepository = agvTaskRepository;
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
    }
}
