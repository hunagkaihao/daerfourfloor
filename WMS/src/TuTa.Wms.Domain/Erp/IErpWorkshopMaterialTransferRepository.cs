using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using TuTa.Wms.Erp.Aggregates;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP车间物料转移仓储接口
    /// </summary>
    public interface IErpWorkshopMaterialTransferRepository : IRepository<ErpWorkshopMaterialTransfer, Guid>
    {
        /// <summary>
        /// 根据启动位置查找转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransfer>> FindByStartLocationAsync(string startLocation);

        /// <summary>
        /// 根据终点位置查找转移任务
        /// </summary>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransfer>> FindByEndLocationAsync(string endLocation);

        /// <summary>
        /// 根据状态查找转移任务
        /// </summary>
        /// <param name="status">任务状态</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransfer>> FindByStatusAsync(MaterialTransferStatus status);

        /// <summary>
        /// 根据位置范围查找转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransfer>> FindByLocationsAsync(string startLocation, string endLocation);

        /// <summary>
        /// 获取待处理的转移任务数量
        /// </summary>
        /// <returns>待处理任务数量</returns>
        Task<int> GetPendingTaskCountAsync();
    }
}
