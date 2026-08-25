using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.StockConsolidations.Dtos;
using Volo.Abp.DependencyInjection;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理线程调度服务。
    /// 服务按单例注册，保证同一WMS进程内最多只有一个整理线程。
    /// </summary>
    public class StockConsolidationService : WmsAppService, IStockConsolidationService, ISingletonDependency
    {
        private readonly object _stateLock = new object();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockConsolidationService> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _runningTask;
        private StockConsolidationStatusDto _status = CreateInitialStatus();

        public StockConsolidationService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<StockConsolidationService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 启动库存整理线程。
        /// </summary>
        public Task<ResponseDto> StartAsync()
        {
            var options = LoadOptions();
            if (!options.Enabled)
            {
                return Task.FromResult(new ResponseDto
                {
                    success = false,
                    message = "库存整理功能未在配置文件中启用"
                });
            }

            lock (_stateLock)
            {
                if (_runningTask != null && !_runningTask.IsCompleted)
                {
                    return Task.FromResult(new ResponseDto
                    {
                        success = false,
                        message = _status.IsStopping ? "库存整理线程正在停止" : "库存整理线程已经运行"
                    });
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                _status = new StockConsolidationStatusDto
                {
                    IsRunning = true,
                    IsStopping = false,
                    Status = "正在启动",
                    StartedAt = DateTime.Now,
                    CompletedGroupCount = 0,
                    CompletedMoveCount = 0
                };
                var cancellationToken = _cancellationTokenSource.Token;
                // 禁止HTTP请求的AsyncLocal/UnitOfWork流入后台线程。
                // 否则请求结束后，后台线程会继续引用已经释放的WmsDbContext。
                using (ExecutionContext.SuppressFlow())
                {
                    _runningTask = Task.Run(() => RunWorkerAsync(cancellationToken));
                }
            }

            return Task.FromResult(new ResponseDto
            {
                success = true,
                message = "库存整理线程已启动"
            });
        }

        /// <summary>
        /// 请求安全停止库存整理线程。
        /// 当前已经下发的AGV任务不会被取消，线程在任务结束后停止。
        /// </summary>
        public Task<ResponseDto> StopAsync()
        {
            lock (_stateLock)
            {
                if (_runningTask == null || _runningTask.IsCompleted)
                {
                    return Task.FromResult(new ResponseDto
                    {
                        success = false,
                        message = "库存整理线程未运行"
                    });
                }

                _status.IsStopping = true;
                _status.Status = "正在停止，等待当前搬运任务结束";
                _cancellationTokenSource.Cancel();
            }

            return Task.FromResult(new ResponseDto
            {
                success = true,
                message = "已请求停止库存整理线程，当前搬运任务完成后不再下发新任务"
            });
        }

        /// <summary>
        /// 获取线程状态快照，避免前端读取到正在修改的共享对象。
        /// </summary>
        public Task<StockConsolidationStatusDto> GetStatusAsync()
        {
            lock (_stateLock)
            {
                var status = CloneStatus(_status);
                status.IsEnabled = LoadOptions().Enabled;
                return Task.FromResult(status);
            }
        }

        /// <summary>
        /// 在线程自己的依赖注入作用域中运行整理Worker。
        /// </summary>
        private async Task RunWorkerAsync(CancellationToken cancellationToken)
        {
            try
            {
                UpdateProgress(new StockConsolidationProgress { Status = "运行中" });
                using var scope = _scopeFactory.CreateScope();
                var worker = scope.ServiceProvider.GetRequiredService<StockConsolidationWorker>();
                await worker.ExecuteAsync(UpdateProgress, cancellationToken).ConfigureAwait(false);

                lock (_stateLock)
                {
                    // Worker已经报告异常停止或主动停止时，保留其状态，不覆盖成已完成。
                    if (_status.Status != "异常停止" && _status.Status != "已停止")
                    {
                        _status.Status = _status.IsStopping ? "已停止" : "已完成";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                lock (_stateLock)
                {
                    _status.Status = "已停止";
                }
            }
            catch (Exception exception)
            {
                // 异常只在后台线程边界转换成中文日志和状态，不向宿主继续抛出，
                // 同时不传入异常对象，避免日志打印大段英文调用堆栈。
                _logger.LogError("库存整理线程异常停止：{错误信息}", exception.Message);
                lock (_stateLock)
                {
                    _status.Status = "异常停止";
                    _status.LastError = exception.Message;
                }
            }
            finally
            {
                lock (_stateLock)
                {
                    _status.IsRunning = false;
                    _status.IsStopping = false;
                    _status.StoppedAt = DateTime.Now;
                    _status.CurrentAction = null;
                    _status.CurrentFromCell = null;
                    _status.CurrentToCell = null;
                }
            }
        }

        /// <summary>
        /// 接收Worker进度并合并到全局状态。
        /// 未提供的字段保留当前值。
        /// </summary>
        private void UpdateProgress(StockConsolidationProgress progress)
        {
            lock (_stateLock)
            {
                _status.Status = progress.Status ?? _status.Status;
                _status.CurrentCellCode = progress.CurrentCellCode ?? _status.CurrentCellCode;
                _status.CurrentGroupBarcode = progress.CurrentGroupBarcode ?? _status.CurrentGroupBarcode;
                _status.CurrentAction = progress.CurrentAction ?? _status.CurrentAction;
                _status.CurrentFromCell = progress.CurrentFromCell ?? _status.CurrentFromCell;
                _status.CurrentToCell = progress.CurrentToCell ?? _status.CurrentToCell;
                _status.CompletedGroupCount = Math.Max(_status.CompletedGroupCount, progress.CompletedGroupCount);
                _status.CompletedMoveCount = Math.Max(_status.CompletedMoveCount, progress.CompletedMoveCount);
                if (!string.IsNullOrWhiteSpace(progress.LastError))
                {
                    _status.LastError = progress.LastError;
                }
            }
        }

        /// <summary>
        /// 从当前配置快照读取库存整理属性。
        /// </summary>
        private StockConsolidationOptions LoadOptions()
        {
            return _configuration.GetSection("StockConsolidation").Get<StockConsolidationOptions>()
                   ?? new StockConsolidationOptions();
        }

        private static StockConsolidationStatusDto CreateInitialStatus()
        {
            return new StockConsolidationStatusDto
            {
                IsEnabled = false,
                IsRunning = false,
                IsStopping = false,
                Status = "未启动"
            };
        }

        private static StockConsolidationStatusDto CloneStatus(StockConsolidationStatusDto status)
        {
            return new StockConsolidationStatusDto
            {
                IsEnabled = status.IsEnabled,
                IsRunning = status.IsRunning,
                IsStopping = status.IsStopping,
                Status = status.Status,
                StartedAt = status.StartedAt,
                StoppedAt = status.StoppedAt,
                CurrentCellCode = status.CurrentCellCode,
                CurrentGroupBarcode = status.CurrentGroupBarcode,
                CurrentAction = status.CurrentAction,
                CurrentFromCell = status.CurrentFromCell,
                CurrentToCell = status.CurrentToCell,
                CompletedGroupCount = status.CompletedGroupCount,
                CompletedMoveCount = status.CompletedMoveCount,
                LastError = status.LastError
            };
        }
    }
}
