using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp.Repositories
{
    /// <summary>
    /// ERP入库单仓储接口
    /// </summary>
    public interface IErpInboundOrderRepository : IRepository<ErpInboundOrder, Guid>
    {
        /// <summary>
        /// 根据入库单号查找入库单
        /// </summary>
        /// <param name="inboundOrderNo">入库单号</param>
        /// <returns>入库单</returns>
        Task<ErpInboundOrder> FindByInboundOrderNoAsync(string inboundOrderNo);

        /// <summary>
        /// 根据仓库代号查找入库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号</param>
        /// <returns>入库单列表</returns>
        Task<List<ErpInboundOrder>> GetByWarehouseCodeAsync(string warehouseCode);

        /// <summary>
        /// 根据状态查找入库单列表
        /// </summary>
        /// <param name="status">入库单状态</param>
        /// <returns>入库单列表</returns>
        Task<List<ErpInboundOrder>> GetByStatusAsync(InboundOrderStatus status);

        /// <summary>
        /// 根据计划入库日期范围查找入库单列表
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>入库单列表</returns>
        Task<List<ErpInboundOrder>> GetByPlanInboundDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 获取入库单列表，包含入库单项
        /// </summary>
        /// <returns>入库单列表</returns>
        Task<List<ErpInboundOrder>> GetListWithItemsAsync();

        /// <summary>
        /// 根据ID获取入库单，包含入库单项
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <returns>入库单</returns>
        Task<ErpInboundOrder> GetWithItemsAsync(Guid id);
    }
}
