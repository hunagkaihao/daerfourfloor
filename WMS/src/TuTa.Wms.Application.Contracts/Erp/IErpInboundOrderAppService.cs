using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP入库单应用服务接口
    /// </summary>
    public interface IErpInboundOrderAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP入库单数据
        /// </summary>
        /// <param name="request">入库单请求数据</param>
        /// <returns>处理结果</returns>
        Task<ErpInboundOrderResponseDto> ReceiveInboundOrderAsync(ErpInboundOrderRequestDto request);

        /// <summary>
        /// 根据ID获取入库单
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <returns>入库单信息</returns>
        Task<ErpInboundOrderDto> GetAsync(Guid id);

        /// <summary>
        /// 根据入库单号获取入库单
        /// </summary>
        /// <param name="inboundOrderNo">入库单号</param>
        /// <returns>入库单信息</returns>
        Task<ErpInboundOrderDto> GetByInboundOrderNoAsync(string inboundOrderNo);

        /// <summary>
        /// 获取入库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号（可选）</param>
        /// <param name="status">状态（可选）</param>
        /// <param name="startDate">开始日期（可选）</param>
        /// <param name="endDate">结束日期（可选）</param>
        /// <returns>入库单列表</returns>
        Task<List<ErpInboundOrderDto>> GetListAsync(
            string warehouseCode = null,
            int? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 更新入库单状态
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateStatusAsync(Guid id, int status);

        /// <summary>
        /// 删除入库单
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
