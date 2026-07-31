using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP出库单应用服务接口
    /// </summary>
    public interface IErpOutboundOrderAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP出库单数据
        /// </summary>
        /// <param name="request">出库单请求数据</param>
        /// <returns>出库单响应结果</returns>
        Task<ErpOutboundOrderResponseDto> ReceiveOutboundOrderAsync(ErpOutboundOrderRequestDto request);

        /// <summary>
        /// 根据ID获取出库单
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <returns>出库单</returns>
        Task<ErpOutboundOrderDto> GetAsync(Guid id);

        /// <summary>
        /// 根据出库单号获取出库单
        /// </summary>
        /// <param name="outboundOrderNo">出库单号</param>
        /// <returns>出库单</returns>
        Task<ErpOutboundOrderDto> GetByOutboundOrderNoAsync(string outboundOrderNo);

        /// <summary>
        /// 获取出库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号</param>
        /// <param name="status">状态</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>出库单列表</returns>
        Task<List<ErpOutboundOrderDto>> GetListAsync(
            string warehouseCode = null,
            int? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 更新出库单状态
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <param name="status">状态</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateStatusAsync(Guid id, int status);

        /// <summary>
        /// 删除出库单
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 根据发货单号创建出库单
        /// </summary>
        /// <param name="deliveryOrderNo">发货单号</param>
        /// <returns>出库单</returns>
        Task<ErpOutboundOrderDto> CreateFromDeliveryOrderAsync(string deliveryOrderNo);

        /// <summary>
        /// 根据条码创建出库记录（已有存货编码则不重复录入）
        /// </summary>
        /// <param name="dto">条码数据</param>
        /// <returns>出库记录</returns>
        Task<ErpOutboundRecordDto> CreateFromBarcodeAsync(CreateFromBarcodeDto dto);
    }
}
