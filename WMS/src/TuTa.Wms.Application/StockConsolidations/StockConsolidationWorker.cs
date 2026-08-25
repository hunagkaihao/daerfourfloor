using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.Cells;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using Volo.Abp.Uow;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理业务Worker。
    /// 所有错误均转换为中文日志和失败结果，打印日志后安全停止，不向上抛出异常。
    /// </summary>
    internal class StockConsolidationWorker
    {
        private readonly IStockRepository _stockRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly IStockService _stockService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockConsolidationWorker> _logger;
        private readonly StockConsolidationPlanner _planner = new StockConsolidationPlanner();

        public StockConsolidationWorker(
            IStockRepository stockRepository,
            ICellRepository cellRepository,
            IAgvTaskRepository agvTaskRepository,
            IStockService stockService,
            IUnitOfWorkManager unitOfWorkManager,
            IConfiguration configuration,
            ILogger<StockConsolidationWorker> logger)
        {
            _stockRepository = stockRepository;
            _cellRepository = cellRepository;
            _agvTaskRepository = agvTaskRepository;
            _stockService = stockService;
            _unitOfWorkManager = unitOfWorkManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 执行完整S型库存整理循环。
        /// 意外的第三方异常也只打印中文日志，然后正常结束Worker。
        /// </summary>
        public async Task ExecuteAsync(
            Action<StockConsolidationProgress> reportProgress,
            CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteCoreAsync(reportProgress, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                LogAndReportStop(reportProgress, "已收到停止请求，库存整理流程已停止。", false);
            }
            catch (Exception exception)
            {
                LogAndReportStop(reportProgress, $"库存整理发生未预期错误：{exception.Message}");
            }
        }

        /// <summary>
        /// 不使用异常控制业务流程的库存整理主循环。
        /// </summary>
        private async Task ExecuteCoreAsync(
            Action<StockConsolidationProgress> reportProgress,
            CancellationToken cancellationToken)
        {
            var options = LoadOptions();
            var initialResult = await BuildSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
            if (!initialResult.IsSuccess)
            {
                LogAndReportStop(reportProgress, initialResult.ErrorMessage);
                return;
            }

            var initialSnapshot = initialResult.Snapshot;
            // 启动时拒绝与现有4F/4B活动任务并行，避免整理途中可用库位集合发生变化。
            var activeManagedPallet = initialSnapshot.Pallets.Values.FirstOrDefault(pallet =>
                pallet.HasActiveTask &&
                (pallet.CellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
                 options.BufferCells.Contains(pallet.CellCode, StringComparer.OrdinalIgnoreCase)));
            if (activeManagedPallet != null)
            {
                LogAndReportStop(
                    reportProgress,
                    $"容器{activeManagedPallet.BoxCode}在库位{activeManagedPallet.CellCode}存在活动任务，请等待任务结束后再启动整理。");
                return;
            }

            var orderedCells = _planner.BuildOrderedCells(initialSnapshot, options);
            if (orderedCells.Count == 0)
            {
                LogAndReportStop(reportProgress, "没有查询到可参与库存整理的4F库位。");
                return;
            }

            var emptyBufferCells = options.BufferCells.Where(cellCode =>
                initialSnapshot.Cells.TryGetValue(cellCode, out var cell) &&
                cell.IsEmpty &&
                string.Equals(cell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase)).ToList();
            if (emptyBufferCells.Count < options.MinimumEmptyBufferCells)
            {
                LogAndReportStop(
                    reportProgress,
                    $"4B周转区至少需要{options.MinimumEmptyBufferCells}个空库位，当前只有{emptyBufferCells.Count}个。");
                return;
            }

            var cursorIndex = 0;
            var currentHole = emptyBufferCells[0];
            var completedGroups = 0;
            var completedMoves = 0;

            while (cursorIndex < orderedCells.Count)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogAndReportStop(reportProgress, "已收到停止请求，库存整理流程已停止。", false);
                    return;
                }

                var snapshotResult = await BuildSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
                if (!snapshotResult.IsSuccess)
                {
                    LogAndReportStop(reportProgress, snapshotResult.ErrorMessage);
                    return;
                }

                var snapshot = snapshotResult.Snapshot;
                var currentOrderedCells = _planner.BuildOrderedCells(snapshot, options);
                if (!orderedCells.SequenceEqual(currentOrderedCells, StringComparer.OrdinalIgnoreCase))
                {
                    LogAndReportStop(reportProgress, "整理期间可用库位集合发生变化，流程已停止，请重新启动整理。");
                    return;
                }

                var groupPlan = _planner.PlanCurrentGroup(
                    snapshot,
                    orderedCells,
                    cursorIndex,
                    currentHole,
                    options);
                if (groupPlan == null)
                {
                    return;
                }

                if (!groupPlan.IsSuccess)
                {
                    LogAndReportStop(reportProgress, groupPlan.ErrorMessage);
                    return;
                }

                reportProgress(new StockConsolidationProgress
                {
                    Status = "运行中",
                    CurrentCellCode = orderedCells[cursorIndex],
                    CurrentGroupBarcode = groupPlan.GroupMaterialCode,
                    CompletedGroupCount = completedGroups,
                    CompletedMoveCount = completedMoves
                });

                foreach (var move in groupPlan.Moves.OrderBy(item => item.Sequence))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        LogAndReportStop(reportProgress, "已收到停止请求，库存整理流程已停止。", false);
                        return;
                    }

                    reportProgress(new StockConsolidationProgress
                    {
                        Status = "运行中",
                        CurrentCellCode = orderedCells[cursorIndex],
                        CurrentGroupBarcode = groupPlan.GroupMaterialCode,
                        CurrentAction = move.MoveType,
                        CurrentFromCell = move.FromCell,
                        CurrentToCell = move.ToCell,
                        CompletedGroupCount = completedGroups,
                        CompletedMoveCount = completedMoves
                    });

                    var moveResult = await ExecuteMoveAndWaitAsync(move, options).ConfigureAwait(false);
                    if (!moveResult.IsSuccess)
                    {
                        LogAndReportStop(reportProgress, moveResult.ErrorMessage);
                        return;
                    }

                    completedMoves++;
                    reportProgress(new StockConsolidationProgress
                    {
                        CompletedGroupCount = completedGroups,
                        CompletedMoveCount = completedMoves
                    });
                }

                cursorIndex = groupPlan.NextCursorIndex;
                currentHole = groupPlan.NextHoleCell;
                completedGroups++;
                reportProgress(new StockConsolidationProgress
                {
                    Status = "运行中",
                    CurrentCellCode = cursorIndex < orderedCells.Count ? orderedCells[cursorIndex] : null,
                    CurrentGroupBarcode = string.Empty,
                    CurrentAction = string.Empty,
                    CurrentFromCell = string.Empty,
                    CurrentToCell = string.Empty,
                    CompletedGroupCount = completedGroups,
                    CompletedMoveCount = completedMoves
                });
            }
        }

        /// <summary>
        /// 创建一条现有WMS搬运任务并等待终态，所有失败均以结果对象返回。
        /// </summary>
        private async Task<StockConsolidationMoveResult> ExecuteMoveAndWaitAsync(
            StockConsolidationMovePlan move,
            StockConsolidationOptions options)
        {
            try
            {
                var beforeResult = await BuildSnapshotAsync(options, CancellationToken.None).ConfigureAwait(false);
                if (!beforeResult.IsSuccess)
                {
                    return FailedMove(beforeResult.ErrorMessage);
                }

                var pallet = ResolvePalletByStockIds(beforeResult.Snapshot, move.StockIds, out var palletError);
                if (pallet == null)
                {
                    return FailedMove(palletError);
                }

                if (!string.Equals(pallet.CellCode, move.FromCell, StringComparison.OrdinalIgnoreCase))
                {
                    return FailedMove($"搬运前数据变化：计划起点{move.FromCell}，当前实际库位{pallet.CellCode}。");
                }

                if (pallet.HasActiveTask)
                {
                    return FailedMove($"容器{pallet.BoxCode}已经存在活动任务。");
                }

                if (!beforeResult.Snapshot.Cells.TryGetValue(move.ToCell, out var targetCell) ||
                    !targetCell.IsEmpty ||
                    !string.Equals(targetCell.CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(targetCell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase))
                {
                    return FailedMove($"目标库位{move.ToCell}当前不是可用空位。");
                }

                var submittedAt = DateTime.Now;
                var createResult = await _stockService.CreateStockTaskV2(
                    pallet.BoxCode,
                    move.FromCell,
                    move.ToCell).ConfigureAwait(false);
                if (!createResult.success)
                {
                    return FailedMove(createResult.message ?? "创建库存整理搬运任务失败。");
                }

                var deadline = DateTime.Now.AddMinutes(Math.Max(1, options.TaskTimeoutMinutes));
                while (DateTime.Now < deadline)
                {
                    var queryResult = await GetLatestAgvTaskAsync(
                        pallet.BoxCode,
                        move.FromCell,
                        move.ToCell,
                        submittedAt).ConfigureAwait(false);
                    if (!queryResult.IsSuccess)
                    {
                        return FailedMove(queryResult.ErrorMessage);
                    }

                    if (!queryResult.IsFound)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.PollIntervalSeconds))).ConfigureAwait(false);
                        continue;
                    }

                    var agvTask = queryResult.Task;
                    if (agvTask.Status == AgvTaskStatus.Complete)
                    {
                        var afterResult = await BuildSnapshotAsync(options, CancellationToken.None).ConfigureAwait(false);
                        if (!afterResult.IsSuccess)
                        {
                            return FailedMove(afterResult.ErrorMessage);
                        }

                        var movedPallet = ResolvePalletByStockIds(afterResult.Snapshot, move.StockIds, out var movedPalletError);
                        if (movedPallet == null)
                        {
                            return FailedMove(movedPalletError);
                        }

                        if (!string.Equals(movedPallet.CellCode, move.ToCell, StringComparison.OrdinalIgnoreCase))
                        {
                            return FailedMove(
                                $"AGV任务已完成，但库存实际位于{movedPallet.CellCode}，预期为{move.ToCell}。");
                        }

                        return SuccessfulMove();
                    }

                    if (agvTask.Status == AgvTaskStatus.Cancel ||
                        agvTask.Status == AgvTaskStatus.Error ||
                        agvTask.Status == AgvTaskStatus.ExceptionComplete)
                    {
                        return FailedMove($"AGV任务{agvTask.ReqCode}进入异常状态{agvTask.Status}。");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.PollIntervalSeconds))).ConfigureAwait(false);
                }

                return FailedMove($"搬运{move.FromCell}到{move.ToCell}超过等待时限。");
            }
            catch (Exception exception)
            {
                return FailedMove($"执行搬运任务时发生错误：{exception.Message}");
            }
        }

        /// <summary>
        /// 查询库存、库位和活动AGV任务，形成当前仓库快照。
        /// </summary>
        private async Task<StockConsolidationSnapshotResult> BuildSnapshotAsync(
            StockConsolidationOptions options,
            CancellationToken cancellationToken)
        {
            try
            {
                using var unitOfWork = _unitOfWorkManager.Begin(true, false);
                var stocks = await _stockRepository.GetListAsync(true, cancellationToken).ConfigureAwait(false);
                var cells = await _cellRepository.GetListAsync(true, cancellationToken).ConfigureAwait(false);
                var activeTasks = await _agvTaskRepository.GetListAsync(task =>
                    task.AgvTaskStatus != AgvTaskStatus.Complete &&
                    task.AgvTaskStatus != AgvTaskStatus.Cancel &&
                    task.AgvTaskStatus != AgvTaskStatus.Error &&
                    task.AgvTaskStatus != AgvTaskStatus.ExceptionComplete).ConfigureAwait(false);
                var activeBoxes = activeTasks
                    .Where(task => !string.IsNullOrWhiteSpace(task.BoxCode))
                    .Select(task => task.BoxCode)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var snapshot = new StockConsolidationSnapshot();
                foreach (var group in stocks
                             .Where(stock => stock.BoxData != null && !string.IsNullOrWhiteSpace(stock.BoxData.BoxCode))
                             .Where(stock => stock.CellData != null && !string.IsNullOrWhiteSpace(stock.CellData.CellCode))
                             .GroupBy(stock => stock.BoxData.BoxCode, StringComparer.OrdinalIgnoreCase))
                {
                    var pallet = CreatePallet(group.Key, group.ToList(), activeBoxes, out var palletError);
                    if (pallet == null)
                    {
                        return FailedSnapshot(palletError);
                    }

                    if (snapshot.Pallets.ContainsKey(pallet.PalletKey))
                    {
                        return FailedSnapshot($"重复托盘标识：{pallet.PalletKey}。");
                    }
                    snapshot.Pallets.Add(pallet.PalletKey, pallet);
                }

                var palletByCell = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pallet in snapshot.Pallets.Values)
                {
                    if (palletByCell.ContainsKey(pallet.CellCode))
                    {
                        return FailedSnapshot($"库位{pallet.CellCode}存在多个托盘。");
                    }
                    palletByCell.Add(pallet.CellCode, pallet.PalletKey);
                }

                foreach (var cell in cells.Where(cell => !string.IsNullOrWhiteSpace(cell.CellCode)))
                {
                    palletByCell.TryGetValue(cell.CellCode, out var palletKey);
                    snapshot.Cells[cell.CellCode] = new StockConsolidationCellSnapshot
                    {
                        CellCode = cell.CellCode,
                        CellStatus = cell.CellStatus.ToString(),
                        RunStatus = cell.RunStatus.ToString(),
                        PalletKey = palletKey
                    };
                }

                var managedCodes = snapshot.Cells.Keys
                    .Where(code => code.StartsWith("4F", StringComparison.OrdinalIgnoreCase))
                    .Concat(options.BufferCells)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
                foreach (var cellCode in managedCodes)
                {
                    if (!snapshot.Cells.TryGetValue(cellCode, out var cell))
                    {
                        continue;
                    }

                    var reportsEmpty = string.Equals(cell.CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase);
                    if (reportsEmpty != cell.IsEmpty)
                    {
                        return FailedSnapshot($"库位{cellCode}的库位状态与库存数据不一致。");
                    }
                }

                await unitOfWork.CompleteAsync().ConfigureAwait(false);
                return new StockConsolidationSnapshotResult
                {
                    IsSuccess = true,
                    Snapshot = snapshot
                };
            }
            catch (OperationCanceledException)
            {
                return FailedSnapshot("已收到停止请求，库存整理流程已停止。");
            }
            catch (Exception exception)
            {
                return FailedSnapshot($"查询库存整理数据时发生错误：{exception.Message}");
            }
        }

        /// <summary>
        /// 在独立UnitOfWork中查询最新AGV任务，查询错误以结果对象返回。
        /// </summary>
        private async Task<StockConsolidationAgvQueryResult> GetLatestAgvTaskAsync(
            string boxCode,
            string fromCell,
            string toCell,
            DateTime submittedAt)
        {
            try
            {
                using var unitOfWork = _unitOfWorkManager.Begin(true, false);
                var tasks = await _agvTaskRepository.GetListAsync(task =>
                    task.BoxCode == boxCode &&
                    task.StartPositionCode == fromCell &&
                    task.EndPositionCode == toCell &&
                    task.CreationTime >= submittedAt.AddMinutes(-1)).ConfigureAwait(false);
                var entity = tasks.OrderByDescending(task => task.CreationTime).FirstOrDefault();
                var taskSnapshot = entity == null
                    ? null
                    : new StockConsolidationAgvTaskSnapshot
                    {
                        ReqCode = entity.ReqCode,
                        Status = entity.AgvTaskStatus
                    };
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
                return new StockConsolidationAgvQueryResult
                {
                    IsSuccess = true,
                    IsFound = taskSnapshot != null,
                    Task = taskSnapshot
                };
            }
            catch (Exception exception)
            {
                return new StockConsolidationAgvQueryResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"查询AGV任务状态时发生错误：{exception.Message}"
                };
            }
        }

        /// <summary>
        /// 按容器内各物料数量确定整理归属。
        /// 数量最多的物料优先；数量相同则取查询顺序中首次出现的物料。
        /// </summary>
        private static StockConsolidationPalletSnapshot CreatePallet(
            string boxCode,
            List<Stock> stocks,
            HashSet<string> activeBoxes,
            out string errorMessage)
        {
            errorMessage = null;
            var cellCodes = stocks
                .Select(stock => stock.CellData.CellCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (cellCodes.Count != 1)
            {
                errorMessage = $"容器{boxCode}的库存分布在多个库位。";
                return null;
            }

            // 保留仓储查询返回顺序；数量相同时，第一个查找到的物料获得整理优先级。
            var orderedStocks = stocks.ToList();
            var materialCandidates = orderedStocks
                .Select((stock, index) => new
                {
                    Stock = stock,
                    Index = index,
                    MaterialCode = stock.Material?.MaterialCode
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.MaterialCode))
                .GroupBy(item => item.MaterialCode, StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    MaterialCode = group.Key,
                    TotalQuantity = group.Sum(item => item.Stock.TotalCountInTime),
                    FirstIndex = group.Min(item => item.Index)
                })
                .OrderByDescending(item => item.TotalQuantity)
                .ThenBy(item => item.FirstIndex)
                .ToList();
            if (materialCandidates.Count == 0)
            {
                errorMessage = $"容器{boxCode}没有可用于整理的物料编码。";
                return null;
            }

            var barcodes = orderedStocks
                .Select(stock => stock.Barcode)
                .Where(barcode => !string.IsNullOrWhiteSpace(barcode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var stockIds = orderedStocks.Select(stock => stock.Id).Distinct().OrderBy(id => id).ToList();
            return new StockConsolidationPalletSnapshot
            {
                PalletKey = string.Join("-", stockIds.Select(id => id.ToString("N"))),
                BoxCode = boxCode,
                CellCode = cellCodes[0],
                StockIds = stockIds,
                Barcodes = barcodes,
                GroupMaterialCode = materialCandidates[0].MaterialCode,
                IsMixedMaterial = materialCandidates.Count > 1,
                HasActiveTask = activeBoxes.Contains(boxCode)
            };
        }

        /// <summary>
        /// 通过原StockId集合重新定位搬运后的当前托盘，失败时返回null和中文错误。
        /// </summary>
        private static StockConsolidationPalletSnapshot ResolvePalletByStockIds(
            StockConsolidationSnapshot snapshot,
            IEnumerable<Guid> stockIds,
            out string errorMessage)
        {
            errorMessage = null;
            var expected = new HashSet<Guid>(stockIds);
            var matches = snapshot.Pallets.Values
                .Where(pallet => expected.IsSubsetOf(pallet.StockIds))
                .ToList();
            if (matches.Count != 1)
            {
                errorMessage = "无法通过原库存ID定位当前唯一托盘。";
                return null;
            }
            return matches[0];
        }

        /// <summary>
        /// 打印中文日志并向前端报告异常停止，不抛出异常。
        /// </summary>
        private void LogAndReportStop(
            Action<StockConsolidationProgress> reportProgress,
            string message,
            bool isError = true)
        {
            if (isError)
            {
                _logger.LogError("库存整理流程停止：{错误信息}", message);
            }
            else
            {
                _logger.LogInformation("库存整理流程停止：{停止原因}", message);
            }

            reportProgress(new StockConsolidationProgress
            {
                Status = isError ? "异常停止" : "已停止",
                LastError = isError ? message : null,
                CurrentAction = string.Empty,
                CurrentFromCell = string.Empty,
                CurrentToCell = string.Empty
            });
        }

        private static StockConsolidationSnapshotResult FailedSnapshot(string message)
        {
            return new StockConsolidationSnapshotResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }

        private static StockConsolidationMoveResult FailedMove(string message)
        {
            return new StockConsolidationMoveResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }

        private static StockConsolidationMoveResult SuccessfulMove()
        {
            return new StockConsolidationMoveResult { IsSuccess = true };
        }

        private StockConsolidationOptions LoadOptions()
        {
            return _configuration.GetSection("StockConsolidation").Get<StockConsolidationOptions>()
                   ?? new StockConsolidationOptions();
        }
    }
}
