namespace FourFloor.Consolidation.Persistence.Entities;

public sealed class ConsolidationMoveEntity
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public ConsolidationPlanEntity? Plan { get; set; }
    public int GroupSequence { get; set; }
    public string GroupBarcode { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string PalletKey { get; set; } = string.Empty;
    public string StockIdsJson { get; set; } = "[]";
    public string FromCell { get; set; } = string.Empty;
    public string ToCell { get; set; } = string.Empty;
    public string MoveType { get; set; } = string.Empty;
    public string Status { get; set; } = MoveStatuses.Waiting;
    public string? CurrentBoxCode { get; set; }
    public int? AgvTaskId { get; set; }
    public string? AgvReqCode { get; set; }
    public string? AgvTaskCode { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
}

public static class MoveStatuses
{
    public const string Waiting = "Waiting";
    public const string Submitting = "Submitting";
    public const string Dispatched = "Dispatched";
    public const string Executing = "Executing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}
