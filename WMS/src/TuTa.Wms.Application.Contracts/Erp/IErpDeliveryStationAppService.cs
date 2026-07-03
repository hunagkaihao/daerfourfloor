using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP收料工位应用服务接口
    /// </summary>
    public interface IErpDeliveryStationAppService : IApplicationService
    {
        /// <summary>
        /// 接收ERP收料工位数据
        /// </summary>
        /// <param name="request">收料工位请求数据</param>
        /// <returns>收料工位响应结果</returns>
        Task<ErpDeliveryStationResponseDto> ReceiveDeliveryStationAsync(ErpDeliveryStationRequestDto request);

        /// <summary>
        /// 根据ID获取收料工位
        /// </summary>
        /// <param name="id">收料工位ID</param>
        /// <returns>收料工位</returns>
        Task<ErpDeliveryStationDto> GetAsync(Guid id);

        /// <summary>
        /// 根据配送位置代号获取收料工位
        /// </summary>
        /// <param name="deliveryCode">配送位置代号</param>
        /// <returns>收料工位</returns>
        Task<ErpDeliveryStationDto> GetByDeliveryCodeAsync(string deliveryCode);

        /// <summary>
        /// 获取收料工位列表
        /// </summary>
        /// <param name="syncType">操作类型</param>
        /// <param name="startTimeStamp">开始时间戳</param>
        /// <param name="endTimeStamp">结束时间戳</param>
        /// <returns>收料工位列表</returns>
        Task<List<ErpDeliveryStationDto>> GetListAsync(
            string syncType = null,
            long? startTimeStamp = null,
            long? endTimeStamp = null);

        /// <summary>
        /// 删除收料工位
        /// </summary>
        /// <param name="id">收料工位ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
