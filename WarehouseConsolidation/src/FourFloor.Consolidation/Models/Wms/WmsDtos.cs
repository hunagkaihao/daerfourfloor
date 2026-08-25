using System.Text.Json.Serialization;

namespace FourFloor.Consolidation.Models.Wms;

public sealed class WmsStockDto
{
    public Guid Id { get; set; }
    public string? Barcode { get; set; }
    public string? ReceivingMaterialBarcode { get; set; }
    public Guid? BoxId { get; set; }
    public string? BoxCode { get; set; }
    public Guid? CellId { get; set; }
    public string? CellCode { get; set; }
    public string? AreaName { get; set; }
    public decimal TotalCountInTime { get; set; }
    public string? Status { get; set; }
    public string? RunStatus { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public bool HasTask { get; set; }
}

public sealed class WmsCellDto
{
    public Guid Id { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseAreaName { get; set; }
    public string? ShelfName { get; set; }
    public string? CellCode { get; set; }
    public string? CellType { get; set; }
    public string? CellStatus { get; set; }
    public string? RunStatus { get; set; }
    public string? BoxCode { get; set; }
    public string? LaneToColumn { get; set; }
    public int? LanePosition { get; set; }
}

public sealed class WmsBoxDto
{
    public Guid Id { get; set; }
    public string? BoxCode { get; set; }
    public string? BoxName { get; set; }
    public string? BoxTypeName { get; set; }
    public string? CellCode { get; set; }
    public string? Status { get; set; }
}

public sealed class WmsAgvTaskDto
{
    public int Id { get; set; }
    public string? ReqCode { get; set; }
    public string? TaskCode { get; set; }
    public string? TaskTyp { get; set; }
    public int AgvTaskStatus { get; set; }
    public string? BoxCode { get; set; }
    public string? StartPositionCode { get; set; }
    public string? EndPositionCode { get; set; }
    public DateTime? CreationTime { get; set; }
}

public sealed class PagedResult<T>
{
    public long TotalCount { get; set; }
    public List<T> Items { get; set; } = [];
}

public sealed class AgvPagedResult
{
    public long TotalCount { get; set; }
    public List<WmsAgvTaskDto> Items { get; set; } = [];
}

public sealed class WmsOperationResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
