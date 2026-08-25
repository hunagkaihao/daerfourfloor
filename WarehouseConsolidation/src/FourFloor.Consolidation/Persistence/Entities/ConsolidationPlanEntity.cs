namespace FourFloor.Consolidation.Persistence.Entities;

public sealed class ConsolidationPlanEntity
{
    public Guid Id { get; set; }
    public string Status { get; set; } = PlanStatuses.Calculated;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime SnapshotTimeUtc { get; set; }
    public int CursorIndex { get; set; }
    public string CurrentHole { get; set; } = string.Empty;
    public string OrderedCellsJson { get; set; } = "[]";
    public string GroupsJson { get; set; } = "[]";
    public string WarningsJson { get; set; } = "[]";
    public string? FailureReason { get; set; }
    public bool PauseRequested { get; set; }
    public List<ConsolidationMoveEntity> Moves { get; set; } = [];
}

public static class PlanStatuses
{
    public const string Calculated = "Calculated";
    public const string Ready = "Ready";
    public const string Executing = "Executing";
    public const string Paused = "Paused";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}
