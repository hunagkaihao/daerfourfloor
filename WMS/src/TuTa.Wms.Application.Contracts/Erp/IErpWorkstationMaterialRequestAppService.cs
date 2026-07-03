using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位叫料任务应用服务接口
    /// </summary>
    public interface IErpWorkstationMaterialRequestAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP工位叫料任务
        /// </summary>
        /// <param name="request">叫料任务请求数据</param>
        /// <returns>处理结果</returns>
        Task<ErpWorkstationMaterialRequestResponseDto> ReceiveMaterialRequestAsync(ErpWorkstationMaterialRequestRequestDto request);

        /// <summary>
        /// 根据ID获取叫料任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>叫料任务信息</returns>
        Task<ErpWorkstationMaterialRequestDto> GetAsync(Guid id);

        /// <summary>
        /// 根据分拣批次获取叫料任务
        /// </summary>
        /// <param name="sortingBatch">分拣批次</param>
        /// <returns>叫料任务信息</returns>
        Task<ErpWorkstationMaterialRequestDto> GetBySortingBatchAsync(string sortingBatch);

        /// <summary>
        /// 获取叫料任务列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>叫料任务列表</returns>
        Task<PagedResultDto<ErpWorkstationMaterialRequestDto>> GetListAsync(ErpWorkstationMaterialRequestQueryDto input);

        /// <summary>
        /// 根据配送点位置获取叫料任务列表
        /// </summary>
        /// <param name="deliveryPointLocation">配送点位置</param>
        /// <returns>叫料任务列表</returns>
        Task<List<ErpWorkstationMaterialRequestDto>> GetByDeliveryPointLocationAsync(string deliveryPointLocation);

        /// <summary>
        /// 根据状态获取叫料任务列表
        /// </summary>
        /// <param name="status">任务状态</param>
        /// <returns>叫料任务列表</returns>
        Task<List<ErpWorkstationMaterialRequestDto>> GetByStatusAsync(int status);

        /// <summary>
        /// 更新叫料任务状态
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <param name="input">状态更新数据</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateStatusAsync(Guid id, ErpWorkstationMaterialRequestStatusUpdateDto input);

        /// <summary>
        /// 删除叫料任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 获取待处理的叫料任务列表
        /// </summary>
        /// <returns>待处理的叫料任务列表</returns>
        Task<List<ErpWorkstationMaterialRequestDto>> GetPendingRequestsAsync();
    }
}
