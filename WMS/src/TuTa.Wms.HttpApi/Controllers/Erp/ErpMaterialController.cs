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
    /// ERP物料控制器
    /// </summary>
    [RemoteService(Name = "ErpMaterial")]
    [Area("Erp")]
    [Route("api/erp/material")]
    public class ErpMaterialController : AbpController, IErpMaterialAppService
    {
        private readonly IErpMaterialAppService _erpMaterialAppService;

        public ErpMaterialController(IErpMaterialAppService erpMaterialAppService)
        {
            _erpMaterialAppService = erpMaterialAppService;
        }

            /// <summary>
            /// 接收ERP物料数据
            /// </summary>
            /// <param name="request">物料请求数据</param>
            /// <returns>物料响应结果</returns>
            [HttpPost("receive")]
            public async Task<ErpMaterialResponseDto> ReceiveMaterialAsync([FromBody] ErpMaterialRequestDto request)
            {
                return await _erpMaterialAppService.ReceiveMaterialAsync(request);
            }

        /// <summary>
        /// 根据ID获取物料
        /// </summary>
        /// <param name="id">物料ID</param>
        /// <returns>物料</returns>
        [HttpGet("{id}")]
        public async Task<ErpMaterialDto> GetAsync(Guid id)
        {
            return await _erpMaterialAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据物料代号获取物料
        /// </summary>
        /// <param name="materialCode">物料代号</param>
        /// <returns>物料</returns>
        [HttpGet("by-material-code/{materialCode}")]
        public async Task<ErpMaterialDto> GetByMaterialCodeAsync(string materialCode)
        {
            return await _erpMaterialAppService.GetByMaterialCodeAsync(materialCode);
        }

        /// <summary>
        /// 获取物料列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>物料列表</returns>
        [HttpGet("list")]
        public async Task<List<ErpMaterialDto>> GetListAsync(
            [FromQuery] string syncType = null,
            [FromQuery] long? startTimeStamp = null,
            [FromQuery] long? endTimeStamp = null)
        {
            return await _erpMaterialAppService.GetListAsync(syncType, startTimeStamp, endTimeStamp);
        }

        /// <summary>
        /// 删除物料
        /// </summary>
        /// <param name="id">物料ID</param>
        /// <returns>是否成功</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpMaterialAppService.DeleteAsync(id);
        }
    }
}
