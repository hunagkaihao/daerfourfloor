namespace FourFloor.Consolidation.Configuration;

public sealed class WmsApiOptions
{
    public const string SectionName = "WmsApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string BearerToken { get; set; } = string.Empty;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public int PageSize { get; set; } = 1000;
    public string StocksPath { get; set; } = "wms/stock/stocksQuery";
    public string CellsPath { get; set; } = "wms/cell/pagedCellsGet";
    public string BoxesPath { get; set; } = "wms/box/pagedBoxesGet";
    public string AgvTasksPath { get; set; } = "wms/agvtask/paged-list";
    public string CreateMoveTaskPath { get; set; } = "wms/stock/createStockTaskV2";
}
