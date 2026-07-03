using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Repositories;
using System.Linq;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP工位叫料任务仓储接口
    /// </summary>
    public interface IErpWorkstationMaterialRequestRepository : IRepository<ErpWorkstationMaterialRequest, Guid>
    {
        /// <summary>
        /// 根据分拣批次查找物料请求任务
        /// </summary>
        /// <param name="sortingBatch">分拣批次</param>
        /// <returns>物料请求任务</returns>
        Task<ErpWorkstationMaterialRequest> FindBySortingBatchAsync(string sortingBatch);

        /// <summary>
        /// 根据配送点位置查找物料请求任务列表
        /// </summary>
        /// <param name="deliveryPointLocation">配送点位置</param>
        /// <returns>物料请求任务列表</returns>
        Task<List<ErpWorkstationMaterialRequest>> FindByDeliveryPointLocationAsync(string deliveryPointLocation);

        /// <summary>
        /// 根据状态查找物料请求任务列表
        /// </summary>
        /// <param name="status">任务状态</param>
        /// <returns>物料请求任务列表</returns>
        Task<List<ErpWorkstationMaterialRequest>> FindByStatusAsync(MaterialRequestStatus status);

        /// <summary>
        /// 根据配送时间范围查找物料请求任务列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>物料请求任务列表</returns>
        Task<List<ErpWorkstationMaterialRequest>> FindByDeliveryTimeRangeAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取待处理的物料请求任务列表
        /// </summary>
        /// <returns>待处理的物料请求任务列表</returns>
        Task<List<ErpWorkstationMaterialRequest>> GetPendingRequestsAsync();

        /// <summary>
        /// 检查分拣批次是否已存在
        /// </summary>
        /// <param name="sortingBatch">分拣批次</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsBySortingBatchAsync(string sortingBatch);
    }
}
