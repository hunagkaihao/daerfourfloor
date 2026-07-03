using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TuTa.Wms.Application.Contracts.Erp;
using TuTa.Wms.Application.Contracts.Erp.IDto;

namespace TuTa.Wms.HttpApi.Controllers.Erp
{
    [Route("api/erp/delivery-order")]
    [ApiController]
    public class ErpDeliveryOrderController : ControllerBase
    {
        private readonly IErpDeliveryOrderAppService _deliveryOrderAppService;

        public ErpDeliveryOrderController(IErpDeliveryOrderAppService deliveryOrderAppService)
        {
            _deliveryOrderAppService = deliveryOrderAppService;
        }

        [HttpGet("list")]
        [SwaggerOperation(summary: "获取发货单列表", Tags = new[] { "ERP Delivery Order" })]
        public async Task<ErpDeliveryOrderListResponseDto> GetDeliveryOrderListAsync(
            int page = 1,
            int pageSize = 10,
            string deliveryOrderNo = null,
            string warehouseCode = null,
            string startDate = null,
            string endDate = null)
        {
            return await _deliveryOrderAppService.GetDeliveryOrderListAsync(page, pageSize, deliveryOrderNo, warehouseCode, startDate, endDate);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(summary: "获取发货单详情", Tags = new[] { "ERP Delivery Order" })]
        public async Task<ErpDeliveryOrderDto> GetDeliveryOrderByIdAsync(Guid id)
        {
            return await _deliveryOrderAppService.GetDeliveryOrderByIdAsync(id);
        }

        [HttpPost]
        [SwaggerOperation(summary: "创建发货单", Tags = new[] { "ERP Delivery Order" })]
        public async Task<ErpDeliveryOrderDto> CreateDeliveryOrderAsync([FromBody] ErpDeliveryOrderCreateDto input)
        {
            return await _deliveryOrderAppService.CreateDeliveryOrderAsync(input);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(summary: "更新发货单", Tags = new[] { "ERP Delivery Order" })]
        public async Task<ErpDeliveryOrderDto> UpdateDeliveryOrderAsync(Guid id, [FromBody] ErpDeliveryOrderCreateDto input)
        {
            return await _deliveryOrderAppService.UpdateDeliveryOrderAsync(id, input);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(summary: "删除发货单", Tags = new[] { "ERP Delivery Order" })]
        public async Task DeleteDeliveryOrderAsync(Guid id)
        {
            await _deliveryOrderAppService.DeleteDeliveryOrderAsync(id);
        }

        [HttpPost("{id}/complete")]
        [SwaggerOperation(summary: "完成发货单", Tags = new[] { "ERP Delivery Order" })]
        public async Task<ErpDeliveryOrderDto> CompleteDeliveryOrderAsync(Guid id)
        {
            return await _deliveryOrderAppService.CompleteDeliveryOrderAsync(id);
        }
    }
}