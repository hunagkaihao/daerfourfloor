using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.AgvTasks.Dtos;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.AgvTasks
{
    public interface IAgvTaskRepository : IRepository<AgvTask, int>
    {
        Task<AgvTask> FindByIdAsync(
            int id, bool isTrack = true, CancellationToken cancellationToken = default);


        Task<AgvTask> FindByReqCodeAsync(
            string reqcode, bool isTrack = true, CancellationToken cancellationToken = default);
        /// <summary>
        /// 分页查询AGV任务列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        Task<(List<AgvTask> Items, long TotalCount)> GetPagedListAsync(AgvTaskPagedQueryDto input);
    }
}
