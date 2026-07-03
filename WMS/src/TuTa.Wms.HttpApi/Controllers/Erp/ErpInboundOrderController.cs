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
    /// ERP入库单控制器
    /// </summary>
    [RemoteService(Name = "ErpInboundOrder")]
    [Route("api/erp/inboundOrder")]
    public class ErpInboundOrderController : AbpController
    {
        private readonly IErpInboundOrderAppService _erpInboundOrderAppService;

        public ErpInboundOrderController(IErpInboundOrderAppService erpInboundOrderAppService)
        {
            _erpInboundOrderAppService = erpInboundOrderAppService;
        }

        /// <summary>
        /// 接收ERP入库单数据
        /// </summary>
        /// <param name="request">入库单请求数据</param>
        /// <returns>处理结果</returns>
        [HttpPost("receive")]
        public async Task<ErpInboundOrderResponseDto> ReceiveInboundOrderAsync([FromBody] ErpInboundOrderRequestDto request)
        {
            return await _erpInboundOrderAppService.ReceiveInboundOrderAsync(request);
        }

        /// <summary>
        /// 根据ID获取入库单
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <returns>入库单信息</returns>
        [HttpGet("{id}")]
        public async Task<ErpInboundOrderDto> GetAsync(Guid id)
        {
            return await _erpInboundOrderAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据入库单号获取入库单
        /// </summary>
        /// <param name="inboundOrderNo">入库单号</param>
        /// <returns>入库单信息</returns>
        [HttpGet("by-order-no/{inboundOrderNo}")]
        public async Task<ErpInboundOrderDto> GetByInboundOrderNoAsync(string inboundOrderNo)
        {
            return await _erpInboundOrderAppService.GetByInboundOrderNoAsync(inboundOrderNo);
        }

        /// <summary>
        /// 获取入库单列表
        /// </summary>
        /// <param name="warehouseCode">仓库代号（可选）</param>
        /// <param name="status">状态（可选）</param>
        /// <param name="startDate">开始日期（可选）</param>
        /// <param name="endDate">结束日期（可选）</param>
        /// <returns>入库单列表</returns>
        [HttpGet("list")]
        public async Task<List<ErpInboundOrderDto>> GetListAsync(
            [FromQuery] string warehouseCode = null,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            return await _erpInboundOrderAppService.GetListAsync(warehouseCode, status, startDate, endDate);
        }

        /// <summary>
        /// 更新入库单状态
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}/status")]
        public async Task<bool> UpdateStatusAsync(Guid id, [FromQuery] int status)
        {
            return await _erpInboundOrderAppService.UpdateStatusAsync(id, status);
        }

        /// <summary>
        /// 删除入库单
        /// </summary>
        /// <param name="id">入库单ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpInboundOrderAppService.DeleteAsync(id);
        }
    }
}
