using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Volo.Abp.Domain.Repositories;
using TuTa.Wms.Erp.Aggregates;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP工位收料仓储接口
    /// </summary>
    public interface IErpWorkstationMaterialReceiptRepository : IRepository<ErpWorkstationMaterialReceipt, Guid>
    {
        /// <summary>
        /// 根据分拣批次号查找收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>收料记录</returns>
        Task<ErpWorkstationMaterialReceipt> FindBySortingBatchAsync(string sortingBatch);

        /// <summary>
        /// 根据收料时间范围查找收料记录
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>收料记录列表</returns>
        Task<List<ErpWorkstationMaterialReceipt>> FindByReceiptTimeRangeAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 检查分拣批次是否已收料
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>是否已收料</returns>
        Task<bool> ExistsBySortingBatchAsync(string sortingBatch);
    }
}
