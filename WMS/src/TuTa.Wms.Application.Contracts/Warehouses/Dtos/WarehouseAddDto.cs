namespace TuTa.Wms.Warehouses.Dtos;

public class WarehouseAddDto
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    public string WarehouseCode { get; set; }
    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WarehouseName { get; set; }
    /// <summary>
    /// 仓库类型
    /// </summary>
    public string WarehouseType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string WarehouseRemark { get; set; }

}
