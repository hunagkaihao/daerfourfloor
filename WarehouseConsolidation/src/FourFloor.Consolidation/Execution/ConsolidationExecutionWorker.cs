using FourFloor.Consolidation.Configuration;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Execution;

public sealed class ConsolidationExecutionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ConsolidationOptions> options,
    ILogger<ConsolidationExecutionWorker> logger) : BackgroundService
{
    private readonly ConsolidationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var executionService = scope.ServiceProvider.GetRequiredService<PlanExecutionService>();
                await executionService.ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "整理执行后台循环发生未处理异常。");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds)),
                stoppingToken);
        }
    }
}
