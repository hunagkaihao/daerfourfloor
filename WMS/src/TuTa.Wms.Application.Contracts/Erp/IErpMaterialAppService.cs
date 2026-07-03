using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP物料应用服务接口
    /// </summary>
    public interface IErpMaterialAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP物料数据
        /// </summary>
        /// <param name="request">物料请求数据</param>
        /// <returns>物料响应结果</returns>
        Task<ErpMaterialResponseDto> ReceiveMaterialAsync(ErpMaterialRequestDto request);

        /// <summary>
        /// 根据ID获取物料
        /// </summary>
        /// <param name="id">物料ID</param>
        /// <returns>物料</returns>
        Task<ErpMaterialDto> GetAsync(Guid id);

        /// <summary>
        /// 根据物料代号获取物料
        /// </summary>
        /// <param name="materialCode">物料代号</param>
        /// <returns>物料</returns>
        Task<ErpMaterialDto> GetByMaterialCodeAsync(string materialCode);

        /// <summary>
        /// 获取物料列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>物料列表</returns>
        Task<List<ErpMaterialDto>> GetListAsync(
            string syncType = null,
            long? startTimeStamp = null,
            long? endTimeStamp = null);

        /// <summary>
        /// 删除物料
        /// </summary>
        /// <param name="id">物料ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
