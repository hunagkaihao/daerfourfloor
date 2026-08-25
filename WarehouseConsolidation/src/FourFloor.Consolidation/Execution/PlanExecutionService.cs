using System.Text.Json;
using FourFloor.Consolidation.Clients;
using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Planning;
using FourFloor.Consolidation.Models.Wms;
using FourFloor.Consolidation.Persistence;
using FourFloor.Consolidation.Persistence.Entities;
using FourFloor.Consolidation.Snapshot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Execution;

public sealed class PlanExecutionService(
    ConsolidationDbContext dbContext,
    WarehouseSnapshotBuilder snapshotBuilder,
    WmsStockClient stockClient,
    WmsAgvTaskClient agvTaskClient,
    IOptions<ConsolidationOptions> options,
    ILogger<PlanExecutionService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConsolidationOptions _options = options.Value;

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled || !_options.ExecutionEnabled)
        {
            return false;
        }

        var plan = await dbContext.Plans
            .Include(item => item.Moves.OrderBy(move => move.Sequence))
            .OrderBy(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync(
                item => item.Status == PlanStatuses.Ready || item.Status == PlanStatuses.Executing,
                cancellationToken);
        if (plan is null)
        {
            return false;
        }

        var activeMove = plan.Moves
            .OrderBy(move => move.Sequence)
            .FirstOrDefault(move => move.Status != MoveStatuses.Completed);
        if (activeMove is null)
        {
            plan.Status = PlanStatuses.Completed;
            plan.UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(plan.Id, null, "PlanCompleted", "全部整理搬运任务已完成。");
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (plan.PauseRequested && activeMove.Status == MoveStatuses.Waiting)
        {
            plan.Status = PlanStatuses.Paused;
            plan.UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(plan.Id, null, "PlanPaused", "方案已在下一条任务下发前暂停。");
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        plan.Status = PlanStatuses.Executing;
        plan.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            if (activeMove.Status == MoveStatuses.Waiting)
            {
                await SubmitMoveAsync(plan, activeMove, cancellationToken);
            }
            else
            {
                await MonitorMoveAsync(plan, activeMove, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "整理方案 {PlanId} 的任务 {MoveId} 执行失败。", plan.Id, activeMove.Id);
            activeMove.Status = MoveStatuses.Failed;
            activeMove.FailureReason = exception.Message;
            plan.Status = PlanStatuses.Failed;
            plan.FailureReason = exception.Message;
            plan.UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(plan.Id, activeMove.Id, "MoveFailed", exception.Message, "Error");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SubmitMoveAsync(
        ConsolidationPlanEntity plan,
        ConsolidationMoveEntity move,
        CancellationToken cancellationToken)
    {
        var snapshot = await snapshotBuilder.BuildAsync(cancellationToken);
        var pallet = ResolveCurrentPallet(snapshot, move);
        if (!string.Equals(pallet.CurrentCellCode, move.FromCell, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"执行前数据已变化：任务起点计划为 {move.FromCell}，当前货物实际位于 {pallet.CurrentCellCode}。");
        }

        if (pallet.HasActiveTask)
        {
            throw new InvalidOperationException($"托盘当前容器 {pallet.CurrentBoxCode} 已有活动任务。");
        }

        if (!snapshot.Cells.TryGetValue(move.ToCell, out var targetCell))
        {
            throw new InvalidOperationException($"目标库位 {move.ToCell} 在 WMS 中不存在。");
        }

        if (!targetCell.IsEnabled || !targetCell.IsEmpty ||
            !string.Equals(targetCell.CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"目标库位 {move.ToCell} 当前不可用：CellStatus={targetCell.CellStatus}，RunStatus={targetCell.RunStatus}。");
        }

        move.CurrentBoxCode = pallet.CurrentBoxCode;
        move.Status = MoveStatuses.Submitting;
        move.SubmittedAtUtc = DateTime.UtcNow;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        AddEvent(
            plan.Id,
            move.Id,
            "MoveSubmitting",
            $"准备调用 CreateStockTaskV2：{pallet.CurrentBoxCode}，{move.FromCell} → {move.ToCell}。");
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await stockClient.CreateMoveTaskAsync(
            pallet.CurrentBoxCode,
            move.FromCell,
            move.ToCell,
            cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message ?? "WMS 拒绝创建整理搬运任务。");
        }

        move.Status = MoveStatuses.Dispatched;
        AddEvent(plan.Id, move.Id, "MoveDispatched", result.Message ?? "WMS 已接受搬运任务。");
        await TryAttachAgvTaskAsync(move, cancellationToken);
    }

    private async Task MonitorMoveAsync(
        ConsolidationPlanEntity plan,
        ConsolidationMoveEntity move,
        CancellationToken cancellationToken)
    {
        if (move.SubmittedAtUtc.HasValue &&
            DateTime.UtcNow - move.SubmittedAtUtc.Value > TimeSpan.FromMinutes(_options.TaskTimeoutMinutes))
        {
            throw new TimeoutException(
                $"任务 {move.Sequence} 已超过配置的 {_options.TaskTimeoutMinutes} 分钟执行时限。");
        }

        var agvTask = await TryAttachAgvTaskAsync(move, cancellationToken);
        if (agvTask is null)
        {
            move.Status = MoveStatuses.Dispatched;
            return;
        }

        if (agvTask.AgvTaskStatus == 9)
        {
            var snapshot = await snapshotBuilder.BuildAsync(cancellationToken);
            var pallet = ResolveCurrentPallet(snapshot, move);
            if (!string.Equals(pallet.CurrentCellCode, move.ToCell, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"AGV 任务已完成，但货物实际位于 {pallet.CurrentCellCode}，预期为 {move.ToCell}。");
            }

            move.Status = MoveStatuses.Completed;
            move.CompletedAtUtc = DateTime.UtcNow;
            plan.UpdatedAtUtc = DateTime.UtcNow;
            AddEvent(plan.Id, move.Id, "MoveCompleted", $"货物已到达 {move.ToCell}。");
            return;
        }

        if (agvTask.AgvTaskStatus >= 10)
        {
            throw new InvalidOperationException(
                $"AGV 任务 {agvTask.Id} 进入异常终态 {agvTask.AgvTaskStatus}。");
        }

        move.Status = MoveStatuses.Executing;
    }

    private async Task<WmsAgvTaskDto?> TryAttachAgvTaskAsync(
        ConsolidationMoveEntity move,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(move.CurrentBoxCode) || !move.SubmittedAtUtc.HasValue)
        {
            return null;
        }

        var task = await agvTaskClient.FindLatestTaskAsync(
            move.CurrentBoxCode,
            move.FromCell,
            move.ToCell,
            move.SubmittedAtUtc.Value,
            cancellationToken);
        if (task is null)
        {
            return null;
        }

        move.AgvTaskId = task.Id;
        move.AgvReqCode = task.ReqCode;
        move.AgvTaskCode = task.TaskCode;
        return task;
    }

    private static PalletSnapshot ResolveCurrentPallet(
        WarehouseSnapshot snapshot,
        ConsolidationMoveEntity move)
    {
        var stockIds = JsonSerializer.Deserialize<List<Guid>>(move.StockIdsJson, JsonOptions) ?? [];
        var palletKeys = stockIds
            .Where(snapshot.PalletKeyByStockId.ContainsKey)
            .Select(stockId => snapshot.PalletKeyByStockId[stockId])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (palletKeys.Count != 1 || !snapshot.Pallets.TryGetValue(palletKeys[0], out var pallet))
        {
            throw new InvalidOperationException(
                $"无法通过原库存 ID 定位任务 {move.Sequence} 当前所在的唯一托盘。");
        }

        return pallet;
    }

    private void AddEvent(Guid planId, Guid? moveId, string eventType, string message, string level = "Information")
    {
        dbContext.Events.Add(new ConsolidationEventEntity
        {
            PlanId = planId,
            MoveId = moveId,
            OccurredAtUtc = DateTime.UtcNow,
            Level = level,
            EventType = eventType,
            Message = message
        });
    }
}
