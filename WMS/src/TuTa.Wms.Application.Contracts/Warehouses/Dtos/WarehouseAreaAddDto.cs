namespace TuTa.Wms.Warehouses.Dtos
{
    public class WarehouseAreaAddDto
    {
        /// <summary>
        /// 仓库分区编码
        /// </summary>
        public string WarehouseAreaCode { get; set; }
        /// <summary>
        /// 仓库分区名称
        /// </summary>
        public string WarehouseAreaName { get; set; }
        /// <summary>
        /// 仓库分区备注
        /// </summary>
        public string WarehouseAreaRemark { get; set; }
        /// <summary>
        /// 仓库分区类型
        /// </summary>
        //public WarehouseAreaType WarehouseAreaType { get; set; }
        /// <summary>
        /// 仓库分区标记
        /// </summary>
        public string WarehouseAreaFlag { get; set; } = null;
        /// <summary>
        /// 排序号
        /// </summary>
        public string WarehouseAreaOrder { get; set; } = null;
        /// <summary>
        /// 仓库分区分组
        /// </summary>
        public string WarehouseAreaGroup { get; set; } = null;
    }
}
