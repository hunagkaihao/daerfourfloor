using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.AgvTasks
{
    public interface IAgvTaskService:IApplicationService
    {

        /// <summary>
        /// CTU回调接口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ResultAgvTaskDto> CtuCallbackAsync(AgvCallBackRequest input);
        /// <summary>
        /// 分页获取AGV任务列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        Task<AgvTaskPagedResultDto> GetPagedListAsync(AgvTaskPagedQueryDto input);
    }
}
