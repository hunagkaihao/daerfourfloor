using System.Text.Json;
using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Planning;
using FourFloor.Consolidation.Persistence;
using FourFloor.Consolidation.Persistence.Entities;
using FourFloor.Consolidation.Planning;
using FourFloor.Consolidation.Snapshot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Services;

public sealed class ConsolidationOrchestrator(
    WarehouseSnapshotBuilder snapshotBuilder,
    ConsolidationPlanner planner,
    PlanSimulator simulator,
    ConsolidationDbContext dbContext,
    IOptions<ConsolidationOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConsolidationOptions _options = options.Value;

    public async Task<PlanDetails> CreatePlanAsync(CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        var activePlanExists = await dbContext.Plans.AnyAsync(
            plan => plan.Status == PlanStatuses.Ready || plan.Status == PlanStatuses.Executing,
            cancellationToken);
        if (activePlanExists)
        {
            throw new InvalidOperationException("已有待执行或执行中的整理方案，不能创建新方案。");
        }

        var snapshot = await snapshotBuilder.BuildAsync(cancellationToken);
        var draft = planner.CreatePlan(snapshot);
        var simulation = simulator.Validate(snapshot, draft);
        if (!simulation.IsValid)
        {
            throw new ConsolidationPlanningException(
                $"整理方案仿真失败：{string.Join("；", simulation.Errors)}");
        }

        var plan = new ConsolidationPlanEntity
        {
            Id = draft.PlanId,
            Status = PlanStatuses.Calculated,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            SnapshotTimeUtc = draft.SnapshotTimeUtc,
            CursorIndex = draft.FinalCursorIndex,
            CurrentHole = draft.CurrentHole,
            OrderedCellsJson = JsonSerializer.Serialize(draft.OrderedUsableCells, JsonOptions),
            GroupsJson = JsonSerializer.Serialize(
                draft.Groups.Select(group => new GroupDetails(
                    group.Sequence,
                    group.GroupBarcode,
                    group.TargetCells.Count,
                    group.TargetCells,
                    group.Moves.Count)).ToList(),
                JsonOptions),
            WarningsJson = JsonSerializer.Serialize(draft.Warnings, JsonOptions),
            Moves = draft.Groups
                .SelectMany(group => group.Moves.Select(move => new ConsolidationMoveEntity
                {
                    Id = Guid.NewGuid(),
                    PlanId = draft.PlanId,
                    GroupSequence = group.Sequence,
                    GroupBarcode = group.GroupBarcode,
                    Sequence = move.Sequence,
                    PalletKey = move.PalletKey,
                    StockIdsJson = JsonSerializer.Serialize(move.StockIds, JsonOptions),
                    FromCell = move.FromCell,
                    ToCell = move.ToCell,
                    MoveType = move.MoveType.ToString(),
                    Status = MoveStatuses.Waiting
                }))
                .ToList()
        };

        dbContext.Plans.Add(plan);
        dbContext.Events.Add(NewEvent(plan.Id, null, "PlanCalculated", $"方案已计算，共 {plan.Moves.Count} 条搬运任务。"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(plan);
    }

    public async Task<PlanDetails?> GetPlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.Plans
            .AsNoTracking()
            .Include(item => item.Moves.OrderBy(move => move.Sequence))
            .FirstOrDefaultAsync(item => item.Id == planId, cancellationToken);
        return plan is null ? null : MapDetails(plan);
    }

    public async Task<List<PlanSummary>> GetPlansAsync(CancellationToken cancellationToken) =>
        await dbContext.Plans
            .AsNoTracking()
            .OrderByDescending(plan => plan.CreatedAtUtc)
            .Select(plan => new PlanSummary(
                plan.Id,
                plan.Status,
                plan.CreatedAtUtc,
                plan.UpdatedAtUtc,
                plan.Moves.Count,
                plan.Moves.Count(move => move.Status == MoveStatuses.Completed),
                plan.FailureReason))
            .Take(100)
            .ToListAsync(cancellationToken);

    public async Task StartAsync(Guid planId, CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();
        if (!_options.ExecutionEnabled)
        {
            throw new InvalidOperationException("配置中的 ExecutionEnabled=false，当前只允许计算和预览。");
        }

        var otherActivePlan = await dbContext.Plans.AnyAsync(
            plan => plan.Id != planId &&
                    (plan.Status == PlanStatuses.Ready || plan.Status == PlanStatuses.Executing),
            cancellationToken);
        if (otherActivePlan)
        {
            throw new InvalidOperationException("已有其他整理方案正在执行或等待执行。");
        }

        var plan = await dbContext.Plans.FirstOrDefaultAsync(item => item.Id == planId, cancellationToken)
                   ?? throw new KeyNotFoundException("整理方案不存在。");
        if (plan.Status is not (PlanStatuses.Calculated or PlanStatuses.Paused or PlanStatuses.Ready))
        {
            throw new InvalidOperationException($"方案状态为 {plan.Status}，不能开始执行。");
        }

        plan.Status = PlanStatuses.Ready;
        plan.PauseRequested = false;
        plan.FailureReason = null;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.Events.Add(NewEvent(plan.Id, null, "PlanReady", "方案已进入待执行状态。"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PauseAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.Plans
                       .Include(item => item.Moves)
                       .FirstOrDefaultAsync(item => item.Id == planId, cancellationToken)
                   ?? throw new KeyNotFoundException("整理方案不存在。");
        plan.PauseRequested = true;
        if (plan.Status == PlanStatuses.Ready)
        {
            plan.Status = PlanStatuses.Paused;
        }
        plan.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.Events.Add(NewEvent(plan.Id, null, "PauseRequested", "已请求暂停；已下发任务继续由 WMS 完成。"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.Plans.FirstOrDefaultAsync(item => item.Id == planId, cancellationToken)
                   ?? throw new KeyNotFoundException("整理方案不存在。");
        if (plan.Moves.Any(move => move.Status is MoveStatuses.Dispatched or MoveStatuses.Executing))
        {
            plan.PauseRequested = true;
            plan.Status = PlanStatuses.Paused;
            plan.FailureReason = "存在已下发任务，未自动调用 WMS 取消接口；请等待当前任务结束后再取消。";
        }
        else
        {
            plan.Status = PlanStatuses.Cancelled;
        }
        plan.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.Events.Add(NewEvent(plan.Id, null, "PlanCancelled", plan.FailureReason ?? "方案已取消。"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureFeatureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("货物整理功能已在配置中关闭。");
        }
    }

    private static ConsolidationEventEntity NewEvent(Guid planId, Guid? moveId, string eventType, string message) => new()
    {
        PlanId = planId,
        MoveId = moveId,
        OccurredAtUtc = DateTime.UtcNow,
        EventType = eventType,
        Message = message
    };

    private static PlanDetails MapDetails(ConsolidationPlanEntity plan) => new(
        plan.Id,
        plan.Status,
        plan.CreatedAtUtc,
        plan.UpdatedAtUtc,
        plan.SnapshotTimeUtc,
        plan.CursorIndex,
        plan.CurrentHole,
        JsonSerializer.Deserialize<List<GroupDetails>>(plan.GroupsJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<List<string>>(plan.WarningsJson, JsonOptions) ?? [],
        plan.FailureReason,
        plan.Moves.OrderBy(move => move.Sequence).Select(move => new MoveDetails(
            move.Id,
            move.GroupSequence,
            move.GroupBarcode,
            move.Sequence,
            move.MoveType,
            move.FromCell,
            move.ToCell,
            move.Status,
            move.CurrentBoxCode,
            move.AgvTaskId,
            move.AgvReqCode,
            move.FailureReason)).ToList());
}

public sealed record PlanSummary(
    Guid Id,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int MoveCount,
    int CompletedMoveCount,
    string? FailureReason);

public sealed record PlanDetails(
    Guid Id,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime SnapshotTimeUtc,
    int CursorIndex,
    string CurrentHole,
    IReadOnlyList<GroupDetails> Groups,
    IReadOnlyList<string> Warnings,
    string? FailureReason,
    IReadOnlyList<MoveDetails> Moves);

public sealed record MoveDetails(
    Guid Id,
    int GroupSequence,
    string GroupBarcode,
    int Sequence,
    string MoveType,
    string FromCell,
    string ToCell,
    string Status,
    string? CurrentBoxCode,
    int? AgvTaskId,
    string? AgvReqCode,
    string? FailureReason);

public sealed record GroupDetails(
    int Sequence,
    string GroupBarcode,
    int PalletCount,
    IReadOnlyList<string> TargetCells,
    int MoveCount);
