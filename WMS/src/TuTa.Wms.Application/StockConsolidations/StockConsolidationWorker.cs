using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
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
        private readonly IBoxRepository _boxRepository;
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly IStockService _stockService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockConsolidationWorker> _logger;
        private readonly StockConsolidationPlanner _planner = new StockConsolidationPlanner();

        public StockConsolidationWorker(
            IStockRepository stockRepository,
            ICellRepository cellRepository,
            IBoxRepository boxRepository,
            IAgvTaskRepository agvTaskRepository,
            IStockService stockService,
            IUnitOfWorkManager unitOfWorkManager,
            IConfiguration configuration,
            ILogger<StockConsolidationWorker> logger)
        {
            _stockRepository = stockRepository;
            _cellRepository = cellRepository;
            _boxRepository = boxRepository;
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
            // 第一步：读取整理配置并建立启动时仓库快照。
            // 快照只保存业务需要的标量，不把EF实体带出UnitOfWork。
            var options = LoadOptions();
            var initialResult = await BuildSnapshotAsync(options, cancellationToken).ConfigureAwait(false);
            if (!initialResult.IsSuccess)
            {
                LogAndReportStop(reportProgress, initialResult.ErrorMessage);
                return;
            }

            var initialSnapshot = initialResult.Snapshot;
            // 启动时拒绝与现有4F/4B活动任务并行，避免整理途中可用库位集合发生变化。
            // 第二步：整理开始前必须保证4F和4B没有其他活动任务。
            // 当前实现没有整仓资源预占能力，与其他搬运并发会改变S型可用库位集合。
            var activeManagedContainer = initialSnapshot.Containers.Values.FirstOrDefault(container =>
                container.HasActiveTask &&
                (container.CellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
                 options.BufferCells.Contains(container.CellCode, StringComparer.OrdinalIgnoreCase)));
            if (activeManagedContainer != null)
            {
                LogAndReportStop(
                    reportProgress,
                    $"容器{activeManagedContainer.BoxCode}在库位{activeManagedContainer.CellCode}存在活动任务，请等待任务结束后再启动整理。");
                return;
            }

            // 第三步：根据真实可用库位生成固定S型顺序。
            // 偶数排由大列到小列、奇数排由小列到大列，同列先二层再一层。
            var orderedCells = _planner.BuildOrderedCells(initialSnapshot, options);
            if (orderedCells.Count == 0)
            {
                LogAndReportStop(reportProgress, "没有查询到可参与库存整理的4F库位。");
                return;
            }

            // 第四步：校验4B至少存在一个启用空位，用作一个空洞轮转的启动周转位。
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

            // 第五步：从S型序列起点逐物料推进。每完成一个物料组后重新读取仓库，
            // 防止使用上一个物料组执行前的过期库存和库位状态。
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

                // 第六步：当前游标库位决定本轮主物料，并生成腾位、归拢、回收动作。
            // 混料容器只归属于数量最多的主物料；已整理前缀不会再次进入规划。
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
                    CurrentMaterialCode = groupPlan.GroupMaterialCode,
                    CompletedGroupCount = completedGroups,
                    CompletedMoveCount = completedMoves
                });

                // 第七步：严格串行执行当前物料组动作。上一条AGV任务未完成前，
                // 不创建下一条任务，避免起点和目标位发生资源竞争。
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
                        CurrentMaterialCode = groupPlan.GroupMaterialCode,
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

                // 第八步：整组完成后推进游标。游标之前的库位成为已整理前缀，
                // 后续所有物料分组和来源选择都不会再使用这些库位。
                cursorIndex = groupPlan.NextCursorIndex;
                currentHole = groupPlan.NextHoleCell;
                completedGroups++;
                reportProgress(new StockConsolidationProgress
                {
                    Status = "运行中",
                    CurrentCellCode = cursorIndex < orderedCells.Count ? orderedCells[cursorIndex] : null,
                    CurrentMaterialCode = string.Empty,
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

                var container = ResolveContainerByStockIds(beforeResult.Snapshot, move.StockIds, out var containerError);
                if (container == null)
                {
                    return FailedMove(containerError);
                }

                if (!string.Equals(container.CellCode, move.FromCell, StringComparison.OrdinalIgnoreCase))
                {
                    return FailedMove($"搬运前数据变化：计划起点{move.FromCell}，当前实际库位{container.CellCode}。");
                }

                if (container.HasActiveTask)
                {
                    return FailedMove($"容器{container.BoxCode}已经存在活动任务。");
                }

                if (!beforeResult.Snapshot.Cells.TryGetValue(move.ToCell, out var targetCell) ||
                    !targetCell.IsEmpty ||
                    !string.Equals(targetCell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase))
                {
                    return FailedMove($"目标库位{move.ToCell}当前不是可用空位。");
                }

                var submittedAt = DateTime.Now;
                // 库存整理只调用专用任务入口。RCS模板统一由配置指定为De03，
                // 不再借用普通入库De01或普通出库De02，避免影响原有业务完成逻辑。
                var createResult = await _stockService.CreateStockConsolidationTask(
                    container.BoxCode,
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
                        container.BoxCode,
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

                        var movedContainer = ResolveContainerByStockIds(afterResult.Snapshot, move.StockIds, out var movedContainerError);
                        if (movedContainer == null)
                        {
                            return FailedMove(movedContainerError);
                        }

                        if (!string.Equals(movedContainer.CellCode, move.ToCell, StringComparison.OrdinalIgnoreCase))
                        {
                            return FailedMove(
                                $"AGV任务已完成，但容器实际位于{movedContainer.CellCode}，预期为{move.ToCell}。");
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
                // 库位占用的唯一判断依据是是否绑定容器：有容器即有货，无容器即为空。
                // 这里只查询4F整理区和配置的4B周转区，不使用CellStatus和库存条数推断占用。
                var managedCells = cells
                    .Where(cell => !string.IsNullOrWhiteSpace(cell.CellCode))
                    .Where(cell =>
                        cell.CellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
                        options.BufferCells.Contains(cell.CellCode, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                var managedCellIds = managedCells.Select(cell => cell.Id).Distinct().ToList();
                var boundBoxes = managedCellIds.Count == 0
                    ? new List<Box>()
                    : await _boxRepository.GetByCellsIdAsync(
                        managedCellIds,
                        false,
                        true,
                        cancellationToken).ConfigureAwait(false);
                var boxesByCellId = boundBoxes
                    .Where(box => box.CellData?.CellId != null)
                    .GroupBy(box => box.CellData.CellId.Value)
                    .ToDictionary(group => group.Key, group => group.ToList());

                // 库存属于容器，一个容器可以有多条库存和多个物料。
                var stocksByBoxId = stocks
                    .Where(stock => stock.BoxData?.BoxId != null)
                    .GroupBy(stock => stock.BoxData.BoxId.Value)
                    .ToDictionary(group => group.Key, group => group.ToList());
                var containerKeyByCell = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var cell in managedCells)
                {
                    if (!boxesByCellId.TryGetValue(cell.Id, out var cellBoxes) || cellBoxes.Count == 0)
                    {
                        // 无容器就是空库位，不要求CellStatus必须为Nohave。
                        continue;
                    }

                    // 一个库位只能绑定一个容器；容器内库存数量不受限制。
                    if (cellBoxes.Count != 1)
                    {
                        var boxCodes = string.Join("、", cellBoxes.Select(box => box.BoxCode));
                        return FailedSnapshot($"库位{cell.CellCode}绑定了多个容器：{boxCodes}。请先处理容器关系。");
                    }

                    var boundBox = cellBoxes[0];
                    if (!stocksByBoxId.TryGetValue(boundBox.Id, out var containerStocks) || containerStocks.Count == 0)
                    {
                        return FailedSnapshot(
                            $"库位{cell.CellCode}已绑定容器{boundBox.BoxCode}，但容器内没有查询到库存物料。");
                    }

                    // 容器内全部库存都应指向容器当前所在库位，避免搬运后只更新部分库存位置。
                    var inconsistentStock = containerStocks.FirstOrDefault(stock =>
                        stock.CellData == null ||
                        !string.Equals(stock.CellData.CellCode, cell.CellCode, StringComparison.OrdinalIgnoreCase));
                    if (inconsistentStock != null)
                    {
                        return FailedSnapshot(
                            $"容器{boundBox.BoxCode}的库存{inconsistentStock.Id}与当前库位{cell.CellCode}绑定不一致。");
                    }

                    var container = CreateContainer(
                        cell.CellCode,
                        boundBox,
                        containerStocks,
                        activeBoxes,
                        out var containerError);
                    if (container == null)
                    {
                        return FailedSnapshot(containerError);
                    }

                    if (snapshot.Containers.ContainsKey(container.ContainerKey))
                    {
                        return FailedSnapshot($"重复容器标识：{container.ContainerKey}。");
                    }
                    if (containerKeyByCell.ContainsKey(container.CellCode))
                    {
                        return FailedSnapshot($"库位{container.CellCode}被重复生成容器快照，整理流程已停止。");
                    }
                    snapshot.Containers.Add(container.ContainerKey, container);
                    containerKeyByCell.Add(container.CellCode, container.ContainerKey);
                }

                foreach (var cell in cells.Where(cell => !string.IsNullOrWhiteSpace(cell.CellCode)))
                {
                    containerKeyByCell.TryGetValue(cell.CellCode, out var containerKey);
                    snapshot.Cells[cell.CellCode] = new StockConsolidationCellSnapshot
                    {
                        CellCode = cell.CellCode,
                        RunStatus = cell.RunStatus.ToString(),
                        ContainerKey = containerKey
                    };
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
        private static StockConsolidationContainerSnapshot CreateContainer(
            string cellCode,
            Box boundBox,
            List<Stock> stocks,
            HashSet<string> activeBoxes,
            out string errorMessage)
        {
            errorMessage = null;
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
                errorMessage = $"库位{cellCode}的容器{boundBox.BoxCode}没有可用于整理的物料编码。";
                return null;
            }

            var barcodes = orderedStocks
                .Select(stock => stock.Barcode)
                .Where(barcode => !string.IsNullOrWhiteSpace(barcode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var stockIds = orderedStocks.Select(stock => stock.Id).Distinct().OrderBy(id => id).ToList();
            return new StockConsolidationContainerSnapshot
            {
                ContainerKey = boundBox.Id.ToString("N"),
                // 以库位当前绑定容器为准，不再使用单条库存中的冗余BoxCode。
                BoxCode = boundBox.BoxCode,
                CellCode = cellCode,
                StockIds = stockIds,
                Barcodes = barcodes,
                GroupMaterialCode = materialCandidates[0].MaterialCode,
                IsMixedMaterial = materialCandidates.Count > 1,
                HasActiveTask = activeBoxes.Contains(boundBox.BoxCode)
            };
        }

        /// <summary>
        /// 通过原StockId集合重新定位搬运后的当前容器，失败时返回null和中文错误。
        /// </summary>
        private static StockConsolidationContainerSnapshot ResolveContainerByStockIds(
            StockConsolidationSnapshot snapshot,
            IEnumerable<Guid> stockIds,
            out string errorMessage)
        {
            errorMessage = null;
            var expected = new HashSet<Guid>(stockIds);
            var matches = snapshot.Containers.Values
                .Where(container => expected.IsSubsetOf(container.StockIds))
                .ToList();
            if (matches.Count != 1)
            {
                errorMessage = "无法通过原库存ID定位当前唯一容器。";
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
