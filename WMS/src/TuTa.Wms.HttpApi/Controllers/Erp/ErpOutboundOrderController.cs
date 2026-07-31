using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Dto;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace TuTa.Wms.HttpApi.Controllers.Erp
{
    /// <summary>
    /// ERP出库单控制器
    /// </summary>
    [RemoteService(Name = "ErpOutboundOrder")]
    [Area("Erp")]
    [Route("api/erp/outboundOrder")]
    public class ErpOutboundOrderController : AbpController, IErpOutboundOrderAppService
    {
        private readonly IErpOutboundOrderAppService _erpOutboundOrderAppService;

        public ErpOutboundOrderController(IErpOutboundOrderAppService erpOutboundOrderAppService)
        {
            _erpOutboundOrderAppService = erpOutboundOrderAppService;
        }

        /// <summary>
        /// 接收ERP出库单数据
        /// </summary>
        /// <param name="request">出库单请求数据</param>
        /// <returns>出库单响应结果</returns>
        [HttpPost("receive")]
        public async Task<ErpOutboundOrderResponseDto> ReceiveOutboundOrderAsync([FromBody] ErpOutboundOrderRequestDto request)
        {
            return await _erpOutboundOrderAppService.ReceiveOutboundOrderAsync(request);
        }

        /// <summary>
        /// 根据ID获取出库单
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <returns>出库单</returns>
        [HttpGet("{id}")]
        public async Task<ErpOutboundOrderDto> GetAsync(Guid id)
        {
            return await _erpOutboundOrderAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据出库单号获取出库单
        /// </summary>
        /// <param name="outboundOrderNo">出库单号</param>
        /// <returns>出库单</returns>
        [HttpGet("by-order-no/{outboundOrderNo}")]
        public async Task<ErpOutboundOrderDto> GetByOutboundOrderNoAsync(string outboundOrderNo)
        {
            return await _erpOutboundOrderAppService.GetByOutboundOrderNoAsync(outboundOrderNo);
        }

        /// <summary>
        /// 获取出库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号</param>
        /// <param name="status">状态</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>出库单列表</returns>
        [HttpGet("list")]
        public async Task<List<ErpOutboundOrderDto>> GetListAsync(
            [FromQuery] string warehouseCode = null,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await _erpOutboundOrderAppService.GetListAsync(warehouseCode, status, startDate, endDate);
        }

        /// <summary>
        /// 更新出库单状态
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <param name="status">状态</param>
        /// <returns>是否成功</returns>
        [HttpPut("{id}/status")]
        public async Task<bool> UpdateStatusAsync(Guid id, [FromQuery] int status)
        {
            return await _erpOutboundOrderAppService.UpdateStatusAsync(id, status);
        }

        /// <summary>
        /// 删除出库单
        /// </summary>
        /// <param name="id">出库单ID</param>
        /// <returns>是否成功</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpOutboundOrderAppService.DeleteAsync(id);
        }

        /// <summary>
        /// 根据发货单号创建出库单
        /// </summary>
        /// <param name="deliveryOrderNo">发货单号</param>
        /// <returns>出库单</returns>
        [HttpPost("create-from-delivery/{deliveryOrderNo}")]
        public async Task<ErpOutboundOrderDto> CreateFromDeliveryOrderAsync(string deliveryOrderNo)
        {
            return await _erpOutboundOrderAppService.CreateFromDeliveryOrderAsync(deliveryOrderNo);
        }

        /// <summary>
        /// 根据条码创建出库记录
        /// </summary>
        /// <param name="dto">条码数据</param>
        /// <returns>出库记录</returns>
        [HttpPost("create-from-barcode")]
        public async Task<ErpOutboundRecordDto> CreateFromBarcodeAsync([FromBody] CreateFromBarcodeDto dto)
        {
            return await _erpOutboundOrderAppService.CreateFromBarcodeAsync(dto);
        }
    }
}
