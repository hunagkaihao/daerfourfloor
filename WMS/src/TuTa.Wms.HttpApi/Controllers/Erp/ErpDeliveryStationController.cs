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
    /// ERP收料工位控制器
    /// </summary>
    [RemoteService(Name = "ErpDeliveryStation")]
    [Area("Erp")]
    [Route("api/erp/deliveryStation")]
    public class ErpDeliveryStationController : AbpController, IErpDeliveryStationAppService
    {
        private readonly IErpDeliveryStationAppService _erpDeliveryStationAppService;

        public ErpDeliveryStationController(IErpDeliveryStationAppService erpDeliveryStationAppService)
        {
            _erpDeliveryStationAppService = erpDeliveryStationAppService;
        }

        /// <summary>
        /// 接收ERP收料工位数据
        /// </summary>
        /// <param name="request">收料工位请求数据</param>
        /// <returns>收料工位响应结果</returns>
        [HttpPost("receive")]
        public async Task<ErpDeliveryStationResponseDto> ReceiveDeliveryStationAsync([FromBody] ErpDeliveryStationRequestDto request)
        {
            return await _erpDeliveryStationAppService.ReceiveDeliveryStationAsync(request);
        }

        /// <summary>
        /// 根据ID获取收料工位
        /// </summary>
        /// <param name="id">收料工位ID</param>
        /// <returns>收料工位</returns>
        [HttpGet("{id}")]
        public async Task<ErpDeliveryStationDto> GetAsync(Guid id)
        {
            return await _erpDeliveryStationAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据配送位置代号获取收料工位
        /// </summary>
        /// <param name="deliveryCode">配送位置代号</param>
        /// <returns>收料工位</returns>
        [HttpGet("by-delivery-code/{deliveryCode}")]
        public async Task<ErpDeliveryStationDto> GetByDeliveryCodeAsync(string deliveryCode)
        {
            return await _erpDeliveryStationAppService.GetByDeliveryCodeAsync(deliveryCode);
        }

        /// <summary>
        /// 获取收料工位列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>收料工位列表</returns>
        [HttpGet("list")]
        public async Task<List<ErpDeliveryStationDto>> GetListAsync(
            [FromQuery] string syncType = null,
            [FromQuery] long? startTimeStamp = null,
            [FromQuery] long? endTimeStamp = null)
        {
            return await _erpDeliveryStationAppService.GetListAsync(syncType, startTimeStamp, endTimeStamp);
        }

        /// <summary>
        /// 删除收料工位
        /// </summary>
        /// <param name="id">收料工位ID</param>
        /// <returns>是否成功</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpDeliveryStationAppService.DeleteAsync(id);
        }
    }
}
