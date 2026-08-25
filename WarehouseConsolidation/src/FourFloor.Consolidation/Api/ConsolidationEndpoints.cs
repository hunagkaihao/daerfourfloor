using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Services;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Api;

public static class ConsolidationEndpoints
{
    public static IEndpointRouteBuilder MapConsolidationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/consolidation").WithTags("四楼货物整理");

        group.MapGet("/config", (IOptions<ConsolidationOptions> options) =>
        {
            var value = options.Value;
            return Results.Ok(new
            {
                value.Enabled,
                value.ExecutionEnabled,
                value.RequireManualConfirmation,
                value.Rows,
                value.LayerOrder,
                value.AllowCrossRow,
                value.BufferCells,
                value.MinimumEmptyBufferCells,
                value.PollIntervalSeconds,
                value.StrictSerialExecution
            });
        });

        group.MapGet("/plans", async (
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
            Results.Ok(await orchestrator.GetPlansAsync(cancellationToken)));

        group.MapGet("/plans/{planId:guid}", async (
            Guid planId,
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var plan = await orchestrator.GetPlanAsync(planId, cancellationToken);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        group.MapPost("/plans", async (
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
            Results.Ok(await orchestrator.CreatePlanAsync(cancellationToken)));

        group.MapPost("/plans/{planId:guid}/execute", async (
            Guid planId,
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            await orchestrator.StartAsync(planId, cancellationToken);
            return Results.Accepted($"/api/consolidation/plans/{planId}");
        });

        group.MapPost("/plans/{planId:guid}/pause", async (
            Guid planId,
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            await orchestrator.PauseAsync(planId, cancellationToken);
            return Results.NoContent();
        });

        group.MapPost("/plans/{planId:guid}/resume", async (
            Guid planId,
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            await orchestrator.StartAsync(planId, cancellationToken);
            return Results.Accepted($"/api/consolidation/plans/{planId}");
        });

        group.MapPost("/plans/{planId:guid}/cancel", async (
            Guid planId,
            ConsolidationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            await orchestrator.CancelAsync(planId, cancellationToken);
            return Results.NoContent();
        });

        return endpoints;
    }
}
