using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TuTa.Wms.Erp;

/// <summary>启动后首次同步，再按配置周期串行同步当天00:00以来的全部来料检验单；每轮使用独立作用域。</summary>
public class MaterialInspectionSyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MaterialInspectionSyncHostedService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    /// <summary>注入作用域工厂、appsettings配置和后台日志。</summary>
    public MaterialInspectionSyncHostedService(IServiceScopeFactory scopeFactory, IConfiguration configuration,
        ILogger<MaterialInspectionSyncHostedService> logger, IHostApplicationLifetime lifetime)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
        _lifetime = lifetime;
    }

    /// <summary>首次立即同步；按每轮开始时间计算配置间隔，超出间隔时完成后再开始下一轮，不启动重叠任务。</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待ABP和宿主初始化完成，再读取数据库；不阻塞Web服务启动。
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = _lifetime.ApplicationStarted.Register(() => started.TrySetResult(true));
        try { await started.Task.WaitAsync(stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
        while (!stoppingToken.IsCancellationRequested)
        {
            var cycleStartedAt = DateTime.UtcNow;
            var interval = 60;
            try
            {
                var options = _configuration.GetSection("Erp:MaterialInspectionSync").Get<MaterialInspectionSyncOptions>()
                    ?? new MaterialInspectionSyncOptions();
                if (options.IntervalMinutes < 1 || options.IntervalMinutes > 10080)
                    _logger.LogWarning("来料检验单同步间隔须为1至10080分钟，当前值={Interval}，本轮使用默认60分钟", options.IntervalMinutes);
                else interval = options.IntervalMinutes;
                if (options.Enabled)
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<ErpAsnAppService>()
                        .SynchronizeMaterialInspectionsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception exception)
            {
                _logger.LogError(exception, "U8来料检验单定时同步失败：{Reason}，保留已保存数据，下一周期重试", exception.Message);
            }
            var delay = TimeSpan.FromMinutes(interval) - (DateTime.UtcNow - cycleStartedAt);
            try { if (delay > TimeSpan.Zero) await Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
        }
    }
}
