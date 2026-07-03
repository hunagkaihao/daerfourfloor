namespace TuTa.Wms.Warehouses.Dtos;
public class PagedWarehouseQueryDto
{
    /// <summary>
    /// 仓库名关键字
    /// </summary>
    public string NameFilter { get; set; }

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int SkipCount => (PageIndex - 1) * PageSize;

    public int MaxResultCount => PageSize;
}

