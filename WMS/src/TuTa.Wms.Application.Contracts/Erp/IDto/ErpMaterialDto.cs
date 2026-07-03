using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP物料DTO
    /// </summary>
    public class ErpMaterialDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 材料代号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string UnitCode { get; set; }

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
