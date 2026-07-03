using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP收料工位仓储接口
    /// </summary>
    public interface IErpDeliveryStationRepository : IRepository<ErpDeliveryStation, Guid>
    {
        /// <summary>
        /// 根据配送位置代号查找收料工位
        /// </summary>
        /// <param name="deliveryCode">配送位置代号</param>
        /// <returns>收料工位</returns>
        Task<ErpDeliveryStation> FindByDeliveryCodeAsync(string deliveryCode);

        /// <summary>
        /// 根据操作类型查找收料工位列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <returns>收料工位列表</returns>
        Task<List<ErpDeliveryStation>> GetBySyncTypeAsync(string syncType);

        /// <summary>
        /// 根据同步时间戳范围查找收料工位列表
        /// </summary>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>收料工位列表</returns>
        Task<List<ErpDeliveryStation>> GetBySyncTimeStampRangeAsync(long startTimeStamp, long endTimeStamp);
    }
}
