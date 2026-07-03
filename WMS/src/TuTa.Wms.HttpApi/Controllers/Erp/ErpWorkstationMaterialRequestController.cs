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
    /// ERP工位叫料任务控制器
    /// </summary>
    [RemoteService(Name = "ErpWorkstationMaterialRequest")]
    [Route("api/erp/workstationMaterialRequest")]
    public class ErpWorkstationMaterialRequestController : AbpController
    {
        private readonly IErpWorkstationMaterialRequestAppService _erpWorkstationMaterialRequestAppService;

        public ErpWorkstationMaterialRequestController(IErpWorkstationMaterialRequestAppService erpWorkstationMaterialRequestAppService)
        {
            _erpWorkstationMaterialRequestAppService = erpWorkstationMaterialRequestAppService;
        }

        /// <summary>
        /// 接收ERP工位叫料任务
        /// </summary>
        /// <param name="request">叫料任务请求数据</param>
        /// <returns>处理结果</returns>
        [HttpPost("receive")]
        public async Task<ErpWorkstationMaterialRequestResponseDto> ReceiveMaterialRequestAsync([FromBody] ErpWorkstationMaterialRequestRequestDto request)
        {
            return await _erpWorkstationMaterialRequestAppService.ReceiveMaterialRequestAsync(request);
        }

        /// <summary>
        /// 根据ID获取叫料任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>叫料任务信息</returns>
        [HttpGet("{id}")]
        public async Task<ErpWorkstationMaterialRequestDto> GetAsync(Guid id)
        {
            return await _erpWorkstationMaterialRequestAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据分拣批次获取叫料任务
        /// </summary>
        /// <param name="sortingBatch">分拣批次</param>
        /// <returns>叫料任务信息</returns>
        [HttpGet("by-sorting-batch/{sortingBatch}")]
        public async Task<ErpWorkstationMaterialRequestDto> GetBySortingBatchAsync(string sortingBatch)
        {
            return await _erpWorkstationMaterialRequestAppService.GetBySortingBatchAsync(sortingBatch);
        }

        /// <summary>
        /// 获取叫料任务列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>叫料任务列表</returns>
        [HttpGet("list")]
        public async Task<PagedResultDto<ErpWorkstationMaterialRequestDto>> GetListAsync([FromQuery] ErpWorkstationMaterialRequestQueryDto input)
        {
            return await _erpWorkstationMaterialRequestAppService.GetListAsync(input);
        }

        /// <summary>
        /// 根据配送点位置获取叫料任务列表
        /// </summary>
        /// <param name="deliveryPointLocation">配送点位置</param>
        /// <returns>叫料任务列表</returns>
        [HttpGet("by-delivery-point/{deliveryPointLocation}")]
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetByDeliveryPointLocationAsync(string deliveryPointLocation)
        {
            return await _erpWorkstationMaterialRequestAppService.GetByDeliveryPointLocationAsync(deliveryPointLocation);
        }

        /// <summary>
        /// 根据状态获取叫料任务列表
        /// </summary>
        /// <param name="status">任务状态</param>
        /// <returns>叫料任务列表</returns>
        [HttpGet("by-status/{status}")]
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetByStatusAsync(int status)
        {
            return await _erpWorkstationMaterialRequestAppService.GetByStatusAsync(status);
        }

        /// <summary>
        /// 更新叫料任务状态
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="input">状态更新数据</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}/status")]
        public async Task<bool> UpdateStatusAsync(Guid id, [FromBody] ErpWorkstationMaterialRequestStatusUpdateDto input)
        {
            return await _erpWorkstationMaterialRequestAppService.UpdateStatusAsync(id, input);
        }

        /// <summary>
        /// 删除叫料任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpWorkstationMaterialRequestAppService.DeleteAsync(id);
        }

        /// <summary>
        /// 获取待处理的叫料任务列表
        /// </summary>
        /// <returns>待处理的叫料任务列表</returns>
        [HttpGet("pending")]
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetPendingRequestsAsync()
        {
            return await _erpWorkstationMaterialRequestAppService.GetPendingRequestsAsync();
        }
    }
}
