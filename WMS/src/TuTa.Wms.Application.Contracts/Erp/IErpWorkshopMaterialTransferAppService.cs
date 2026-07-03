using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP车间物料转移应用服务接口
    /// </summary>
    public interface IErpWorkshopMaterialTransferAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP车间移库AGV任务
        /// </summary>
        /// <param name="request">转移任务请求</param>
        /// <returns>处理结果</returns>
        Task<ErpWorkshopMaterialTransferResponseDto> ReceiveMaterialTransferTaskAsync(ErpWorkshopMaterialTransferRequestDto request);

        /// <summary>
        /// 根据ID获取转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>转移任务信息</returns>
        Task<ErpWorkshopMaterialTransferDto> GetAsync(Guid id);

        /// <summary>
        /// 根据启动位置获取转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransferDto>> GetByStartLocationAsync(string startLocation);

        /// <summary>
        /// 根据终点位置获取转移任务
        /// </summary>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        Task<List<ErpWorkshopMaterialTransferDto>> GetByEndLocationAsync(string endLocation);

        /// <summary>
        /// 获取转移任务列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>转移任务列表</returns>
        Task<PagedResultDto<ErpWorkshopMaterialTransferDto>> GetListAsync(ErpWorkshopMaterialTransferQueryDto input);

        /// <summary>
        /// 更新转移任务状态
        /// </summary>
        /// <param name="input">状态更新请求</param>
        /// <returns>更新结果</returns>
        Task<ErpWorkshopMaterialTransferResponseDto> UpdateStatusAsync(ErpWorkshopMaterialTransferStatusUpdateDto input);

        /// <summary>
        /// 删除转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 获取待处理的转移任务数量
        /// </summary>
        /// <returns>待处理任务数量</returns>
        Task<int> GetPendingTaskCountAsync();
    }
}
