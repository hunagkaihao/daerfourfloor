using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Dto;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.HttpApi.Controllers.Erp
{
    /// <summary>
    /// ERP车间物料转移控制器
    /// </summary>
    [RemoteService(Name = "ErpWorkshopMaterialTransfer")]
    [Route("api/erp/workshopMaterialTransfer")]
    public class ErpWorkshopMaterialTransferController : AbpController
    {
        private readonly IErpWorkshopMaterialTransferAppService _erpWorkshopMaterialTransferAppService;

        public ErpWorkshopMaterialTransferController(IErpWorkshopMaterialTransferAppService erpWorkshopMaterialTransferAppService)
        {
            _erpWorkshopMaterialTransferAppService = erpWorkshopMaterialTransferAppService;
        }

        /// <summary>
        /// 接收ERP车间移库AGV任务
        /// </summary>
        /// <param name="request">转移任务请求</param>
        /// <returns>处理结果</returns>
        [HttpPost("receive")]
        public async Task<ErpWorkshopMaterialTransferResponseDto> ReceiveMaterialTransferTaskAsync([FromBody] ErpWorkshopMaterialTransferRequestDto request)
        {
            return await _erpWorkshopMaterialTransferAppService.ReceiveMaterialTransferTaskAsync(request);
        }

        /// <summary>
        /// 根据ID获取转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>转移任务信息</returns>
        [HttpGet("{id}")]
        public async Task<ErpWorkshopMaterialTransferDto> GetAsync(Guid id)
        {
            return await _erpWorkshopMaterialTransferAppService.GetAsync(id);
        }

        /// <summary>
        /// 根据启动位置获取转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <returns>转移任务列表</returns>
        [HttpGet("byStartLocation/{startLocation}")]
        public async Task<ErpWorkshopMaterialTransferDto[]> GetByStartLocationAsync(string startLocation)
        {
            var result = await _erpWorkshopMaterialTransferAppService.GetByStartLocationAsync(startLocation);
            return result.ToArray();
        }

        /// <summary>
        /// 根据终点位置获取转移任务
        /// </summary>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        [HttpGet("byEndLocation/{endLocation}")]
        public async Task<ErpWorkshopMaterialTransferDto[]> GetByEndLocationAsync(string endLocation)
        {
            var result = await _erpWorkshopMaterialTransferAppService.GetByEndLocationAsync(endLocation);
            return result.ToArray();
        }

        /// <summary>
        /// 获取转移任务列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>转移任务列表</returns>
        [HttpGet]
        public async Task<PagedResultDto<ErpWorkshopMaterialTransferDto>> GetListAsync([FromQuery] ErpWorkshopMaterialTransferQueryDto input)
        {
            return await _erpWorkshopMaterialTransferAppService.GetListAsync(input);
        }

        /// <summary>
        /// 更新转移任务状态
        /// </summary>
        /// <param name="input">状态更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("status")]
        public async Task<ErpWorkshopMaterialTransferResponseDto> UpdateStatusAsync([FromBody] ErpWorkshopMaterialTransferStatusUpdateDto input)
        {
            return await _erpWorkshopMaterialTransferAppService.UpdateStatusAsync(input);
        }

        /// <summary>
        /// 删除转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _erpWorkshopMaterialTransferAppService.DeleteAsync(id);
        }

        /// <summary>
        /// 获取待处理的转移任务数量
        /// </summary>
        /// <returns>待处理任务数量</returns>
        [HttpGet("pendingCount")]
        public async Task<int> GetPendingTaskCountAsync()
        {
            return await _erpWorkshopMaterialTransferAppService.GetPendingTaskCountAsync();
        }
    }
}
