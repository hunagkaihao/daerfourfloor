using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP出库单仓储接口
    /// </summary>
    public interface IErpOutboundOrderRepository : IRepository<ErpOutboundOrder, Guid>
    {
        /// <summary>
        /// 根据出库单号查找出库单
        /// </summary>
        /// <param name="outboundOrderNo">出库单号</param>
        /// <returns>出库单</returns>
        Task<ErpOutboundOrder> FindByOutboundOrderNoAsync(string outboundOrderNo);

        /// <summary>
        /// 根据仓库代号查找出库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号</param>
        /// <returns>出库单列表</returns>
        Task<List<ErpOutboundOrder>> GetByWarehouseCodeAsync(string warehouseCode);

        /// <summary>
        /// 根据状态查找出库单列表
        /// </summary>
        /// <param name="status">出库单状态</param>
        /// <returns>出库单列表</returns>
        Task<List<ErpOutboundOrder>> GetByStatusAsync(OutboundOrderStatus status);

        /// <summary>
        /// 根据计划出库日期范围查找出库单列表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>出库单列表</returns>
        Task<List<ErpOutboundOrder>> GetByPlanOutboundDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 获取出库单列表，包含出库单项
        /// </summary>
        /// <returns>出库单列表</returns>
        Task<List<ErpOutboundOrder>> GetListWithItemsAsync();

        /// <summary>
        /// 根据ID获取出库单，包含出库单项
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <returns>出库单</returns>
        Task<ErpOutboundOrder> GetWithItemsAsync(Guid id);
    }
}
