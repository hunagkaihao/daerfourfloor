using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Dtos;
using TuTa.Wms.Application.Contracts.Shared;
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

        /// <summary>
        /// 取消AGV任务：下发给RCS取消任务、容器解绑、恢复库位状态、删除组盘库存
        /// </summary>
        Task<ResponseDto> CancelAgvTaskAsync(int taskId);

        /// <summary>
        /// 仓位与容器的关系绑定, 容器类型编号写入仓位表。
        /// </summary>
        /// <returns></returns>
        Task<ResultAgvTaskDto> BindCtnrAndBinAsync(string reqCode, string stgBinCode, string ctnrType = "5", string ctnrCode = null, string indBind = "0");
    }
}
