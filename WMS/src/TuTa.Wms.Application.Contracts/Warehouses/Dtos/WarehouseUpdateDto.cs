namespace TuTa.Wms.Warehouses.Dtos;

public class WarehouseUpdateDto
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    public string WarehouseCodeNew { get; set; }
    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WarehouseNameNew { get; set; }
    /// <summary>
    /// 仓库类型
    /// </summary>
    public string WarehouseTypeNew { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string WarehouseRemarkNew { get; set; }
}

