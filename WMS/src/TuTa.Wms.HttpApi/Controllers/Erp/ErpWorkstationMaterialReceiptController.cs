using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Dto;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace TuTa.Wms.HttpApi.Controllers.Erp
{
    /// <summary>
    /// ERP工位收料控制器
    /// </summary>
    [RemoteService(Name = "ErpWorkstationMaterialReceipt")]
    [Route("api/erp/workstationMaterialReceipt")]
    public class ErpWorkstationMaterialReceiptController : AbpController
    {
        private readonly IErpWorkstationMaterialReceiptAppService _erpWorkstationMaterialReceiptAppService;

        public ErpWorkstationMaterialReceiptController(IErpWorkstationMaterialReceiptAppService erpWorkstationMaterialReceiptAppService)
        {
            _erpWorkstationMaterialReceiptAppService = erpWorkstationMaterialReceiptAppService;
        }

        /// <summary>
        /// 接收工位收料信息
        /// </summary>
        /// <param name="request">收料请求数据</param>
        /// <returns>处理结果</returns>
        [HttpPost("receive")]
        public async Task<ErpWorkstationMaterialReceiptResponseDto> ReceiveMaterialReceiptAsync([FromBody] ErpWorkstationMaterialReceiptRequestDto request)
        {
            return await _erpWorkstationMaterialReceiptAppService.ReceiveMaterialReceiptAsync(request);
        }

        /// <summary>
        /// 根据ID获取收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>收料记录信息</returns>
        [HttpGet("{id}")]
        public async Task<ErpWorkstationMaterialReceiptDto> GetAsync(Guid id)
        {
            return await _erpWorkstationMaterialReceiptAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据分拣批次号获取收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>收料记录信息</returns>
        [HttpGet("by-sorting-batch/{sortingBatch}")]
        public async Task<ErpWorkstationMaterialReceiptDto> GetBySortingBatchAsync(string sortingBatch)
        {
            return await _erpWorkstationMaterialReceiptAppService.GetBySortingBatchAsync(sortingBatch);
        }

        /// <summary>
        /// 获取收料记录列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>收料记录列表</returns>
        [HttpGet("list")]
        public async Task<PagedResultDto<ErpWorkstationMaterialReceiptDto>> GetListAsync([FromQuery] ErpWorkstationMaterialReceiptQueryDto input)
        {
            return await _erpWorkstationMaterialReceiptAppService.GetListAsync(input);
        }

        /// <summary>
        /// 根据时间范围获取收料记录列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>收料记录列表</returns>
        [HttpGet("by-time-range")]
        public async Task<List<ErpWorkstationMaterialReceiptDto>> GetByTimeRangeAsync(
            [FromQuery] DateTime startTime, 
            [FromQuery] DateTime endTime)
        {
            return await _erpWorkstationMaterialReceiptAppService.GetByTimeRangeAsync(startTime, endTime);
        }

        /// <summary>
        /// 删除收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpWorkstationMaterialReceiptAppService.DeleteAsync(id);
        }
    }
}
