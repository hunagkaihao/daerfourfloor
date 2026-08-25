namespace FourFloor.Consolidation.Models.Planning;

public sealed record CellPosition(
    string CellCode,
    int Row,
    int Column,
    int Layer,
    int SequenceIndex,
    bool IsBuffer = false);

public sealed class PalletSnapshot
{
    public required string PalletKey { get; init; }
    public required string CurrentBoxCode { get; set; }
    public required string CurrentCellCode { get; set; }
    public required List<Guid> StockIds { get; init; }
    public required List<string> Barcodes { get; init; }
    public required string GroupBarcode { get; init; }
    public string? MaterialCode { get; init; }
    public bool HasActiveTask { get; init; }
}

public sealed class WarehouseSnapshot
{
    public DateTime CapturedAtUtc { get; init; } = DateTime.UtcNow;
    public required IReadOnlyDictionary<string, CellState> Cells { get; init; }
    public required IReadOnlyDictionary<string, PalletSnapshot> Pallets { get; init; }
    public required IReadOnlyDictionary<Guid, string> PalletKeyByStockId { get; init; }
}

public sealed class CellState
{
    public required string CellCode { get; init; }
    public required string CellStatus { get; init; }
    public required string RunStatus { get; init; }
    public string? CellType { get; init; }
    public string? PalletKey { get; set; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(PalletKey);
    public bool IsEnabled => string.Equals(RunStatus, "Enable", StringComparison.OrdinalIgnoreCase);
}

public enum ConsolidationMoveType
{
    Consolidate = 1,
    EvictToBuffer = 2,
    RelocateBlocker = 3,
    ReturnFromBuffer = 4
}

public sealed class PlannedMove
{
    public required int Sequence { get; init; }
    public required string PalletKey { get; init; }
    public required List<Guid> StockIds { get; init; }
    public required string GroupBarcode { get; init; }
    public required string FromCell { get; init; }
    public required string ToCell { get; init; }
    public required ConsolidationMoveType MoveType { get; init; }
}

public sealed class PlannedGroup
{
    public required int Sequence { get; init; }
    public required string GroupBarcode { get; init; }
    public required List<string> TargetCells { get; init; }
    public required List<PlannedMove> Moves { get; init; }
}

public sealed class ConsolidationPlanDraft
{
    public Guid PlanId { get; init; } = Guid.NewGuid();
    public DateTime SnapshotTimeUtc { get; init; }
    public required List<string> OrderedUsableCells { get; init; }
    public required List<PlannedGroup> Groups { get; init; }
    public required string CurrentHole { get; init; }
    public required int FinalCursorIndex { get; init; }
    public required List<string> Warnings { get; init; }
}
