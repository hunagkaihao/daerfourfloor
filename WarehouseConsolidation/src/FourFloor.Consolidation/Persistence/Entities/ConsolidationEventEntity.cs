namespace FourFloor.Consolidation.Persistence.Entities;

public sealed class ConsolidationEventEntity
{
    public long Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid? MoveId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string Level { get; set; } = "Information";
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DataJson { get; set; }
}
