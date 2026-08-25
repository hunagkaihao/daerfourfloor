using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 库存整理每日自动启动服务。
    /// 本服务只负责按配置时间触发，与移动端按钮共用StockConsolidationService.StartAsync，
    /// 因而自动触发和手动触发使用同一把进程内互斥锁，不会创建两个整理线程。
    /// </summary>
    public class StockConsolidationScheduleService : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

        private readonly IStockConsolidationService _stockConsolidationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockConsolidationScheduleService> _logger;
        private DateTime? _handledDate;
        private string _lastInvalidTime;

        public StockConsolidationScheduleService(
            IStockConsolidationService stockConsolidationService,
            IConfiguration configuration,
            ILogger<StockConsolidationScheduleService> logger)
        {
            _stockConsolidationService = stockConsolidationService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 启动后每30秒读取一次最新配置。
        /// 首次运行若已经晚于当天配置时间，会把当天标记为已处理而不补跑；
        /// 后续自然跨日后，才会在新一天的配置时间触发一次。
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitializeHandledDate();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TryStartForCurrentDayAsync().ConfigureAwait(false);
                    await Task.Delay(CheckInterval, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // WMS宿主停止时正常结束调度服务，不打印异常日志，也不影响正在安全停止的整理线程。
                    return;
                }
                catch (Exception exception)
                {
                    // 调度器自身异常不能终止WMS宿主；只打印简体中文消息，下一个检查周期继续尝试。
                    _logger.LogError("库存整理每日调度检查失败：{错误信息}", exception.Message);
                    await Task.Delay(CheckInterval, stoppingToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 初始化当天处理标记，明确禁止服务启动后的补跑行为。
        /// 例如WMS在22:30启动且配置时间为22:00，当天不会自动整理，下一天22:00才会触发。
        /// </summary>
        private void InitializeHandledDate()
        {
            var options = LoadOptions();
            if (!TryParseDailyStartTime(options.DailyStartTime, out var startTime))
            {
                return;
            }

            var now = DateTime.Now;
            if (now.TimeOfDay >= startTime)
            {
                _handledDate = now.Date;
            }
        }

        /// <summary>
        /// 当前日期到达配置时间后只处理一次。
        /// 即使功能关闭、自动启动关闭或手动线程已在运行，当天也不会在后续轮询中反复触发。
        /// </summary>
        private async Task TryStartForCurrentDayAsync()
        {
            var options = LoadOptions();
            if (!TryParseDailyStartTime(options.DailyStartTime, out var startTime))
            {
                LogInvalidTimeOnce(options.DailyStartTime);
                return;
            }

            _lastInvalidTime = null;
            var now = DateTime.Now;
            if (_handledDate == now.Date || now.TimeOfDay < startTime)
            {
                return;
            }

            // 先标记当天已处理，再尝试启动，保证任何失败结果都不会在30秒后无限重试并重复下发。
            _handledDate = now.Date;
            if (!options.Enabled)
            {
                _logger.LogInformation("库存整理功能未启用，跳过{日期}的自动整理", now.ToString("yyyy-MM-dd"));
                return;
            }

            if (!options.AutoStartEnabled)
            {
                _logger.LogInformation("库存整理自动启动未启用，跳过{日期}的定时触发", now.ToString("yyyy-MM-dd"));
                return;
            }

            var result = await _stockConsolidationService.StartAsync().ConfigureAwait(false);
            if (result.success)
            {
                _logger.LogInformation(
                    "库存整理已按每日配置时间{启动时间}自动启动",
                    startTime.ToString(@"hh\:mm"));
            }
            else
            {
                _logger.LogWarning("库存整理定时启动未执行：{原因}", result.message);
            }
        }

        /// <summary>
        /// 从配置文件读取最新选项，使修改自动启动开关和时间后无需重新创建调度服务。
        /// </summary>
        private StockConsolidationOptions LoadOptions()
        {
            return _configuration.GetSection("StockConsolidation").Get<StockConsolidationOptions>()
                   ?? new StockConsolidationOptions();
        }

        /// <summary>
        /// 只接受明确的24小时制HH:mm，例如22:00，避免不同服务器区域设置产生解析差异。
        /// </summary>
        private static bool TryParseDailyStartTime(string value, out TimeSpan startTime)
        {
            return TimeSpan.TryParseExact(
                value,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out startTime)
                && startTime >= TimeSpan.Zero
                && startTime < TimeSpan.FromDays(1);
        }

        /// <summary>
        /// 同一个错误配置值只打印一次，防止30秒轮询持续刷屏。
        /// </summary>
        private void LogInvalidTimeOnce(string value)
        {
            var invalidValue = value ?? "<空>";
            if (string.Equals(_lastInvalidTime, invalidValue, StringComparison.Ordinal))
            {
                return;
            }

            _lastInvalidTime = invalidValue;
            _logger.LogError(
                "库存整理每日启动时间配置无效：{配置值}，必须使用24小时制HH:mm格式，例如22:00",
                invalidValue);
        }
    }
}
