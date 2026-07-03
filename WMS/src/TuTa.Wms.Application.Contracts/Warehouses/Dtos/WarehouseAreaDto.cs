using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Warehouses.Dtos
{
    public class WarehouseAreaDto : EntityDto<int>
    {
        /// <summary>
        /// 仓库ID
        /// </summary>
        public Guid WarehouseId { get; set; }
        /// <summary>
        /// 仓库分区编码
        /// </summary>
        public string WarehouseAreaCode { get; set; }
        /// <summary>
        /// 仓库分区名称
        /// </summary>
        public string WarehouseAreaName { get; set; }
        /// <summary>
        /// 仓库分区标记
        /// </summary>
        public string WarehouseAreaFlag { get; set; }
        /// <summary>
        /// 仓库分区类型
        /// </summary>
        //public WarehouseAreaType WarehouseAreaType { get; set; }
        /// <summary>
        /// 仓库分区备注
        /// </summary>
        public string WarehouseAreaRemark { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public string WarehouseAreaOrder { get; set; }
        /// <summary>
        /// 仓库分区分组
        /// </summary>
        public string WarehouseAreaGroup { get; set; }
    }
}
