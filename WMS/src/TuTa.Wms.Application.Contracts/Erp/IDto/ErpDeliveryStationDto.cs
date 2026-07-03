using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP收料工位DTO
    /// </summary>
    public class ErpDeliveryStationDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 配送位置代号
        /// </summary>
        public string DeliveryCode { get; set; }

        /// <summary>
        /// 同步时间戳
        /// </summary>
        public long SyncTimeStamp { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string SyncType { get; set; }
    }
}
