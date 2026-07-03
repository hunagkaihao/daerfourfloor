using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP物料仓储接口
    /// </summary>
    public interface IErpMaterialRepository : IRepository<ErpMaterial, Guid>
    {
        /// <summary>
        /// 根据物料代号查找物料
        /// </summary>
        /// <param name="materialCode">物料代号</param>
        /// <returns>物料</returns>
        Task<ErpMaterial> FindByMaterialCodeAsync(string materialCode);

        /// <summary>
        /// 根据操作类型查找物料列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <returns>物料列表</returns>
        Task<List<ErpMaterial>> GetBySyncTypeAsync(string syncType);

        /// <summary>
        /// 根据同步时间戳范围查找物料列表
        /// </summary>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>物料列表</returns>
        Task<List<ErpMaterial>> GetBySyncTimeStampRangeAsync(long startTimeStamp, long endTimeStamp);

        /// <summary>
        /// 获取所有未接收的物料
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>未接收的物料列表</returns>
        Task<List<ErpMaterial>> GetAllUnReceivedMaterialsAsync(CancellationToken cancellationToken = default);
    }
}
