using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位收料应用服务接口
    /// </summary>
    public interface IErpWorkstationMaterialReceiptAppService : IApplicationService
    {
        /// <summary>
        /// 接收工位收料信息
        /// </summary>
        /// <param name="request">收料请求数据</param>
        /// <returns>处理结果</returns>
        Task<ErpWorkstationMaterialReceiptResponseDto> ReceiveMaterialReceiptAsync(ErpWorkstationMaterialReceiptRequestDto request);

        /// <summary>
        /// 根据ID获取收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>收料记录信息</returns>
        Task<ErpWorkstationMaterialReceiptDto> GetAsync(Guid id);

        /// <summary>
        /// 根据分拣批次号获取收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>收料记录信息</returns>
        Task<ErpWorkstationMaterialReceiptDto> GetBySortingBatchAsync(string sortingBatch);

        /// <summary>
        /// 获取收料记录列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>收料记录列表</returns>
        Task<PagedResultDto<ErpWorkstationMaterialReceiptDto>> GetListAsync(ErpWorkstationMaterialReceiptQueryDto input);

        /// <summary>
        /// 根据时间范围获取收料记录列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>收料记录列表</returns>
        Task<List<ErpWorkstationMaterialReceiptDto>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 删除收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
