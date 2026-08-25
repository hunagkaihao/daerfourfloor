using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理业务Worker。
    /// 每次线程运行只创建一个Worker实例，并严格串行下发搬运任务。
    /// </summary>
    internal class StockConsolidationWorker
    {
        private readonly IStockRepository _stockRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly IStockService _stockService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockConsolidationWorker> _logger;
        private readonly StockConsolidationPlanner _planner = new StockConsolidationPlanner();

        public StockConsolidationWorker(
            IStockRepository stockRepository,
            ICellRepository cellRepository,
            IAgvTaskRepository agvTaskRepository,
            IStockService stockService,
            IConfiguration configuration,
            ILogger<StockConsolidationWorker> logger)
        {
            _stockRepository = stockRepository;
            _cellRepository = cellRepository;
            _agvTaskRepository = agvTaskRepository;
            _stockService = stockService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 执行完整S型库存整理循环。
        /// 每完成一个物料组后重新查询仓库，避免长时间使用过期快照。
        /// </summary>
        public async Task ExecuteAsync(
            Action<StockConsolidationProgress> reportProgress,
            CancellationToken cancellationToken)
        {
            var options = LoadOptions();
            var initialSnapshot = await BuildSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
            // 启动时拒绝与现有4F/4B活动任务并行，避免整理途中可用库位集合发生变化。
            var activeManagedPallet = initialSnapshot.Pallets.Values.FirstOrDefault(pallet =>
                pallet.HasActiveTask &&
                (pallet.CellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
                 options.BufferCells.Contains(pallet.CellCode, StringComparer.OrdinalIgnoreCase)));
            if (activeManagedPallet != null)
            {
                throw new InvalidOperationException(
                    $"容器{activeManagedPallet.BoxCode}在库位{activeManagedPallet.CellCode}存在活动任务，请等待任务结束后再启动整理。");
            }

            var orderedCells = _planner.BuildOrderedCells(initialSnapshot, options);
            if (orderedCells.Count == 0)
            {
                throw new InvalidOperationException("没有查询到可参与库存整理的4F库位。");
            }

            var emptyBufferCount = options.BufferCells.Count(cellCode =>
                initialSnapshot.Cells.TryGetValue(cellCode, out var cell) &&
                cell.IsEmpty &&
                string.Equals(cell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase));
            if (emptyBufferCount < options.MinimumEmptyBufferCells)
            {
                throw new InvalidOperationException(
                    $"4B周转区至少需要{options.MinimumEmptyBufferCells}个空库位，当前只有{emptyBufferCount}个。");
            }

            var cursorIndex = 0;
            var currentHole = options.BufferCells.First(cellCode =>
                initialSnapshot.Cells.TryGetValue(cellCode, out var cell) &&
                cell.IsEmpty &&
                string.Equals(cell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase));
            var completedGroups = 0;
            var completedMoves = 0;

            while (cursorIndex < orderedCells.Count)
            {
                // 停止请求只在新物料组或新动作开始前生效。
                cancellationToken.ThrowIfCancellationRequested();

                var snapshot = await BuildSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
                var currentOrderedCells = _planner.BuildOrderedCells(snapshot, options);
                if (!orderedCells.SequenceEqual(currentOrderedCells, StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("整理期间可用库位集合发生变化，线程已停止，请重新启动规划。");
                }

                var groupPlan = _planner.PlanCurrentGroup(
                    snapshot,
                    orderedCells,
                    cursorIndex,
                    currentHole,
                    options);
                if (groupPlan == null)
                {
                    break;
                }

                reportProgress(new StockConsolidationProgress
                {
                    Status = "运行中",
                    CurrentCellCode = orderedCells[cursorIndex],
                    CurrentGroupBarcode = groupPlan.GroupBarcode,
                    CompletedGroupCount = completedGroups,
                    CompletedMoveCount = completedMoves
                });

                foreach (var move in groupPlan.Moves.OrderBy(item => item.Sequence))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    reportProgress(new StockConsolidationProgress
                    {
                        Status = "运行中",
                        CurrentCellCode = orderedCells[cursorIndex],
                        CurrentGroupBarcode = groupPlan.GroupBarcode,
                        CurrentAction = move.MoveType,
                        CurrentFromCell = move.FromCell,
                        CurrentToCell = move.ToCell,
                        CompletedGroupCount = completedGroups,
                        CompletedMoveCount = completedMoves
                    });

                    await ExecuteMoveAndWaitAsync(move, options).ConfigureAwait(false);
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
        /// 创建一条现有WMS搬运任务，并等待AGV任务进入终态。
        /// 等待期间不响应取消令牌，保证已下发任务完成后再安全停止。
        /// </summary>
        private async Task ExecuteMoveAndWaitAsync(
            StockConsolidationMovePlan move,
            StockConsolidationOptions options)
        {
            var beforeMove = await BuildSnapshotAsync(options, CancellationToken.None).ConfigureAwait(false);
            var pallet = ResolvePalletByStockIds(beforeMove, move.StockIds);
            if (!string.Equals(pallet.CellCode, move.FromCell, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"搬运前数据变化：计划起点{move.FromCell}，当前实际库位{pallet.CellCode}。");
            }

            if (pallet.HasActiveTask)
            {
                throw new InvalidOperationException($"容器{pallet.BoxCode}已经存在活动任务。");
            }

            if (!beforeMove.Cells.TryGetValue(move.ToCell, out var targetCell) ||
                !targetCell.IsEmpty ||
                !string.Equals(targetCell.CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(targetCell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"目标库位{move.ToCell}当前不是可用空位。");
            }

            var submittedAt = DateTime.Now;
            var result = await _stockService.CreateStockTaskV2(
                pallet.BoxCode,
                move.FromCell,
                move.ToCell).ConfigureAwait(false);
            if (!result.success)
            {
                throw new InvalidOperationException(result.message ?? "创建库存整理搬运任务失败。");
            }

            var deadline = DateTime.Now.AddMinutes(Math.Max(1, options.TaskTimeoutMinutes));
            while (DateTime.Now < deadline)
            {
                var tasks = await _agvTaskRepository.GetListAsync(task =>
                    task.BoxCode == pallet.BoxCode &&
                    task.StartPositionCode == move.FromCell &&
                    task.EndPositionCode == move.ToCell &&
                    task.CreationTime >= submittedAt.AddMinutes(-1)).ConfigureAwait(false);
                var agvTask = tasks.OrderByDescending(task => task.CreationTime).FirstOrDefault();
                if (agvTask == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.PollIntervalSeconds))).ConfigureAwait(false);
                    continue;
                }

                if (agvTask.AgvTaskStatus == AgvTaskStatus.Complete)
                {
                    var afterMove = await BuildSnapshotAsync(options, CancellationToken.None).ConfigureAwait(false);
                    var movedPallet = ResolvePalletByStockIds(afterMove, move.StockIds);
                    if (!string.Equals(movedPallet.CellCode, move.ToCell, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"AGV任务已完成，但库存实际位于{movedPallet.CellCode}，预期为{move.ToCell}。");
                    }
                    return;
                }

                if (agvTask.AgvTaskStatus == AgvTaskStatus.Cancel ||
                    agvTask.AgvTaskStatus == AgvTaskStatus.Error ||
                    agvTask.AgvTaskStatus == AgvTaskStatus.ExceptionComplete)
                {
                    throw new InvalidOperationException(
                        $"AGV任务{agvTask.ReqCode}进入异常状态{agvTask.AgvTaskStatus}。");
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.PollIntervalSeconds))).ConfigureAwait(false);
            }

            throw new TimeoutException($"搬运{move.FromCell}到{move.ToCell}超过等待时限。");
        }

        /// <summary>
        /// 查询库存、库位和活动AGV任务，形成当前仓库快照。
        /// </summary>
        private async Task<StockConsolidationSnapshot> BuildSnapshotAsync(
            StockConsolidationOptions options,
            CancellationToken cancellationToken)
        {
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
            var pallets = stocks
                .Where(stock => stock.BoxData != null && !string.IsNullOrWhiteSpace(stock.BoxData.BoxCode))
                .Where(stock => stock.CellData != null && !string.IsNullOrWhiteSpace(stock.CellData.CellCode))
                .GroupBy(stock => stock.BoxData.BoxCode, StringComparer.OrdinalIgnoreCase)
                .Select(group => CreatePallet(group.Key, group.ToList(), activeBoxes))
                .Where(pallet => pallet != null)
                .ToList();

            foreach (var pallet in pallets)
            {
                if (snapshot.Pallets.ContainsKey(pallet.PalletKey))
                {
                    throw new InvalidOperationException($"重复托盘标识：{pallet.PalletKey}");
                }
                snapshot.Pallets.Add(pallet.PalletKey, pallet);
            }

            var palletByCell = pallets
                .GroupBy(pallet => pallet.CellCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count() == 1
                        ? group.First().PalletKey
                        : throw new InvalidOperationException($"库位{group.Key}存在多个托盘。"),
                    StringComparer.OrdinalIgnoreCase);

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

            // 只检查本次整理管理的4F和配置4B库位，防止把状态不一致库位当成空位。
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
                    throw new InvalidOperationException($"库位{cellCode}的库位状态与库存数据不一致。");
                }
            }

            return snapshot;
        }

        /// <summary>
        /// 按容器内库存创建托盘快照，并按创建时间确定第一个Barcode。
        /// </summary>
        private static StockConsolidationPalletSnapshot CreatePallet(
            string boxCode,
            List<Stock> stocks,
            HashSet<string> activeBoxes)
        {
            var cellCodes = stocks
                .Select(stock => stock.CellData.CellCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (cellCodes.Count != 1)
            {
                throw new InvalidOperationException($"容器{boxCode}的库存分布在多个库位。");
            }

            var orderedStocks = stocks.OrderBy(stock => stock.CreationTime).ThenBy(stock => stock.Id).ToList();
            var barcodes = orderedStocks
                .Select(stock => stock.Barcode)
                .Where(barcode => !string.IsNullOrWhiteSpace(barcode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (barcodes.Count == 0)
            {
                return null;
            }

            var stockIds = orderedStocks.Select(stock => stock.Id).Distinct().OrderBy(id => id).ToList();
            return new StockConsolidationPalletSnapshot
            {
                PalletKey = string.Join("-", stockIds.Select(id => id.ToString("N"))),
                BoxCode = boxCode,
                CellCode = cellCodes[0],
                StockIds = stockIds,
                Barcodes = barcodes,
                GroupBarcode = barcodes[0],
                HasActiveTask = activeBoxes.Contains(boxCode)
            };
        }

        /// <summary>
        /// 通过原StockId集合重新定位搬运后的当前容器和库位。
        /// </summary>
        private static StockConsolidationPalletSnapshot ResolvePalletByStockIds(
            StockConsolidationSnapshot snapshot,
            IEnumerable<Guid> stockIds)
        {
            var expected = new HashSet<Guid>(stockIds);
            var matches = snapshot.Pallets.Values
                .Where(pallet => expected.IsSubsetOf(pallet.StockIds))
                .ToList();
            if (matches.Count != 1)
            {
                throw new InvalidOperationException("无法通过原库存ID定位当前唯一托盘。");
            }
            return matches[0];
        }

        private StockConsolidationOptions LoadOptions()
        {
            return _configuration.GetSection("StockConsolidation").Get<StockConsolidationOptions>()
                   ?? new StockConsolidationOptions();
        }
    }
}
