using System;
using System.Collections.Generic;
using System.Linq;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理规划器。
    /// 负责生成S型库位顺序，并为当前同物料组生成一个空位轮转搬运计划。
    /// </summary>
    internal class StockConsolidationPlanner
    {
        private readonly StockConsolidationCellParser _cellParser = new StockConsolidationCellParser();

        /// <summary>
        /// 按偶数排倒序、奇数排正序、同列二层后的一层生成S型顺序。
        /// </summary>
        public List<string> BuildOrderedCells(
            StockConsolidationSnapshot snapshot,
            StockConsolidationOptions options)
        {
            var rowSet = new HashSet<int>(options.Rows);
            var layerPriority = options.LayerOrder
                .Select((layer, index) => new { layer, index })
                .ToDictionary(item => item.layer, item => item.index);

            var positions = snapshot.Cells.Values
                .Where(cell => string.Equals(cell.RunStatus, "Enable", StringComparison.OrdinalIgnoreCase))
                .Select(cell => _cellParser.TryParse(cell.CellCode, out var position) ? position : null)
                .Where(position => position != null)
                .Where(position => rowSet.Contains(position.Row))
                .Where(position => layerPriority.ContainsKey(position.Layer))
                .Where(position => !IsExcluded(position, options.ExcludedRanges))
                .ToList();

            var result = new List<string>();
            foreach (var row in options.Rows)
            {
                var rowPositions = positions.Where(position => position.Row == row);
                rowPositions = row % 2 == 0
                    ? rowPositions.OrderByDescending(position => position.Column)
                        .ThenBy(position => layerPriority[position.Layer])
                    : rowPositions.OrderBy(position => position.Column)
                        .ThenBy(position => layerPriority[position.Layer]);

                result.AddRange(rowPositions.Select(position => position.CellCode));
            }

            return result;
        }

        /// <summary>
        /// 从当前游标开始规划一个同物料组。
        /// 目标块允许跨层、跨列和跨排，并通过一个空位交替完成腾位和归拢。
        /// </summary>
        public StockConsolidationGroupPlan PlanCurrentGroup(
            StockConsolidationSnapshot snapshot,
            IReadOnlyList<string> orderedCells,
            int cursorIndex,
            string currentHole,
            StockConsolidationOptions options)
        {
            if (cursorIndex >= orderedCells.Count)
            {
                return null;
            }

            // 只使用WMS中真实存在的4B配置库位，避免把配置错误的库位当成空位。
            var validBufferCells = options.BufferCells
                .Where(snapshot.Cells.ContainsKey)
                .Where(cellCode => string.Equals(
                    snapshot.Cells[cellCode].RunStatus,
                    "Enable",
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
            var managedCells = new HashSet<string>(orderedCells, StringComparer.OrdinalIgnoreCase);
            managedCells.UnionWith(validBufferCells);
            var finalizedCells = new HashSet<string>(
                orderedCells.Take(cursorIndex),
                StringComparer.OrdinalIgnoreCase);
            var containers = snapshot.Containers.Values
                .Where(container => managedCells.Contains(container.CellCode))
                .ToDictionary(container => container.ContainerKey, CloneContainer, StringComparer.OrdinalIgnoreCase);
            if (containers.Count == 0)
            {
                return null;
            }

            var occupancy = managedCells.ToDictionary(
                cellCode => cellCode,
                cellCode => snapshot.Cells.TryGetValue(cellCode, out var cell) ? cell.ContainerKey : null,
                StringComparer.OrdinalIgnoreCase);

            var cursorCell = orderedCells[cursorIndex];
            StockConsolidationContainerSnapshot seedContainer = null;
            if (occupancy.TryGetValue(cursorCell, out var cursorContainerKey) &&
                !string.IsNullOrWhiteSpace(cursorContainerKey))
            {
                seedContainer = containers[cursorContainerKey];
            }

            // 当前游标为空时，从后续S型库位寻找第一个有货容器填补空位。
            seedContainer ??= containers.Values
                .Where(container => orderedCells.Skip(cursorIndex).Contains(container.CellCode, StringComparer.OrdinalIgnoreCase))
                .OrderBy(container => IndexOf(orderedCells, container.CellCode))
                .FirstOrDefault();
            seedContainer ??= containers.Values
                .Where(container => validBufferCells.Contains(container.CellCode, StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (seedContainer == null)
            {
                return null;
            }

            var groupContainers = containers.Values
                // 已整理前缀中的容器不再进入任何物料组，混料容器因此不会被重复搬运。
                .Where(container => !finalizedCells.Contains(container.CellCode))
                .Where(container => string.Equals(
                    container.GroupMaterialCode,
                    seedContainer.GroupMaterialCode,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (groupContainers.Any(container => container.HasActiveTask))
            {
                return FailedPlan($"物料组{seedContainer.GroupMaterialCode}存在活动任务，无法整理。");
            }

            if (cursorIndex + groupContainers.Count > orderedCells.Count)
            {
                return FailedPlan($"物料组{seedContainer.GroupMaterialCode}所需库位超过剩余可用容量。");
            }

            var targetCells = orderedCells.Skip(cursorIndex).Take(groupContainers.Count).ToList();
            var moves = new List<StockConsolidationMovePlan>();
            var moveSequence = 1;

            // 先填充目标块中已经存在的空位，以产生后续腾位所需的新空洞。
            foreach (var targetCell in targetCells.Where(cellCode => string.IsNullOrWhiteSpace(occupancy[cellCode])).ToList())
            {
                var source = FindSource(groupContainers, targetCells, validBufferCells);
                if (source == null)
                {
                    return FailedPlan($"目标位{targetCell}为空，但找不到物料组{seedContainer.GroupMaterialCode}的来源容器。");
                }

                var sourceCell = source.CellCode;
                AddMove(moves, source, sourceCell, targetCell,
                    validBufferCells.Contains(sourceCell, StringComparer.OrdinalIgnoreCase) ? "暂存物料回收" : "归拢",
                    moveSequence++);
                var applyError = ApplyMove(source, sourceCell, targetCell, occupancy);
                if (!string.IsNullOrWhiteSpace(applyError))
                {
                    return FailedPlan(applyError);
                }
                currentHole = sourceCell;
            }

            // 对目标块中的异物执行“异物到空洞、目标物料到异物原位”的交替搬运。
            foreach (var targetCell in targetCells)
            {
                var targetContainerKey = occupancy[targetCell];
                if (string.IsNullOrWhiteSpace(targetContainerKey))
                {
                    continue;
                }

                var blocker = containers[targetContainerKey];
                if (string.Equals(blocker.GroupMaterialCode, seedContainer.GroupMaterialCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (blocker.HasActiveTask)
                {
                    return FailedPlan($"目标位{targetCell}的阻挡容器存在活动任务。");
                }

                if (!IsUsableHole(currentHole, occupancy, targetCells, orderedCells.Take(cursorIndex)))
                {
                    currentHole = FindEmptyCell(validBufferCells, occupancy)
                                  ?? FindEmptyCell(orderedCells.Skip(cursorIndex), occupancy);
                    if (string.IsNullOrWhiteSpace(currentHole))
                    {
                        return FailedPlan("没有可用于腾位的空库位。");
                    }
                }

                var blockerFrom = blocker.CellCode;
                var blockerTo = currentHole;
                AddMove(moves, blocker, blockerFrom, blockerTo,
                    validBufferCells.Contains(blockerTo, StringComparer.OrdinalIgnoreCase) ? "腾位到4B" : "腾位",
                    moveSequence++);
                var blockerError = ApplyMove(blocker, blockerFrom, blockerTo, occupancy);
                if (!string.IsNullOrWhiteSpace(blockerError))
                {
                    return FailedPlan(blockerError);
                }

                var source = FindSource(groupContainers, targetCells, validBufferCells);
                if (source == null)
                {
                    return FailedPlan($"目标位{targetCell}已腾空，但找不到对应目标物料。");
                }
                var sourceCell = source.CellCode;
                AddMove(moves, source, sourceCell, targetCell,
                    validBufferCells.Contains(sourceCell, StringComparer.OrdinalIgnoreCase) ? "暂存物料回收" : "归拢",
                    moveSequence++);
                var sourceError = ApplyMove(source, sourceCell, targetCell, occupancy);
                if (!string.IsNullOrWhiteSpace(sourceError))
                {
                    return FailedPlan(sourceError);
                }
                currentHole = sourceCell;
            }

            return new StockConsolidationGroupPlan
            {
                GroupMaterialCode = seedContainer.GroupMaterialCode,
                TargetCells = targetCells,
                Moves = moves,
                NextCursorIndex = cursorIndex + groupContainers.Count,
                NextHoleCell = currentHole
            };
        }

        private bool IsExcluded(
            StockConsolidationCellPosition position,
            IEnumerable<StockConsolidationCellRange> ranges)
        {
            foreach (var range in ranges)
            {
                if (!_cellParser.TryParse(range.From, out var from) ||
                    !_cellParser.TryParse(range.To, out var to) ||
                    position.Row != from.Row || from.Row != to.Row)
                {
                    continue;
                }

                if (position.Column >= Math.Min(from.Column, to.Column) &&
                    position.Column <= Math.Max(from.Column, to.Column))
                {
                    return true;
                }
            }

            return false;
        }

        private static StockConsolidationContainerSnapshot FindSource(
            IEnumerable<StockConsolidationContainerSnapshot> groupContainers,
            IEnumerable<string> targetCells,
            IEnumerable<string> bufferCells)
        {
            var targetSet = new HashSet<string>(targetCells, StringComparer.OrdinalIgnoreCase);
            var bufferSet = new HashSet<string>(bufferCells, StringComparer.OrdinalIgnoreCase);
            return groupContainers
                .Where(container => !targetSet.Contains(container.CellCode))
                .OrderByDescending(container => bufferSet.Contains(container.CellCode))
                .ThenBy(container => container.CellCode)
                .FirstOrDefault();
        }

        private static void AddMove(
            ICollection<StockConsolidationMovePlan> moves,
            StockConsolidationContainerSnapshot container,
            string fromCell,
            string toCell,
            string moveType,
            int sequence)
        {
            moves.Add(new StockConsolidationMovePlan
            {
                Sequence = sequence,
                ContainerKey = container.ContainerKey,
                StockIds = container.StockIds.ToList(),
                GroupMaterialCode = container.GroupMaterialCode,
                FromCell = fromCell,
                ToCell = toCell,
                MoveType = moveType
            });
        }

        private static string ApplyMove(
            StockConsolidationContainerSnapshot container,
            string fromCell,
            string toCell,
            IDictionary<string, string> occupancy)
        {
            if (!occupancy.TryGetValue(fromCell, out var currentContainer) ||
                !string.Equals(currentContainer, container.ContainerKey, StringComparison.OrdinalIgnoreCase))
            {
                return $"容器{container.BoxCode}不在计划起点{fromCell}。";
            }

            if (occupancy.TryGetValue(toCell, out var targetContainer) && !string.IsNullOrWhiteSpace(targetContainer))
            {
                return $"计划目标位{toCell}不是空位。";
            }

            occupancy[fromCell] = null;
            occupancy[toCell] = container.ContainerKey;
            container.CellCode = toCell;
            return null;
        }

        /// <summary>
        /// 创建失败规划结果，由Worker统一打印中文日志并停止。
        /// </summary>
        private static StockConsolidationGroupPlan FailedPlan(string message)
        {
            return new StockConsolidationGroupPlan
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }

        private static bool IsUsableHole(
            string hole,
            IReadOnlyDictionary<string, string> occupancy,
            IEnumerable<string> targetCells,
            IEnumerable<string> finalizedCells)
        {
            if (string.IsNullOrWhiteSpace(hole) ||
                !occupancy.TryGetValue(hole, out var occupant) ||
                !string.IsNullOrWhiteSpace(occupant))
            {
                return false;
            }

            return !targetCells.Contains(hole, StringComparer.OrdinalIgnoreCase) &&
                   !finalizedCells.Contains(hole, StringComparer.OrdinalIgnoreCase);
        }

        private static string FindEmptyCell(
            IEnumerable<string> cells,
            IReadOnlyDictionary<string, string> occupancy)
        {
            return cells.FirstOrDefault(cell =>
                occupancy.TryGetValue(cell, out var occupant) && string.IsNullOrWhiteSpace(occupant));
        }

        private static int IndexOf(IReadOnlyList<string> cells, string target)
        {
            for (var index = 0; index < cells.Count; index++)
            {
                if (string.Equals(cells[index], target, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return int.MaxValue;
        }

        private static StockConsolidationContainerSnapshot CloneContainer(StockConsolidationContainerSnapshot source)
        {
            return new StockConsolidationContainerSnapshot
            {
                ContainerKey = source.ContainerKey,
                BoxCode = source.BoxCode,
                CellCode = source.CellCode,
                StockIds = source.StockIds.ToList(),
                Barcodes = source.Barcodes.ToList(),
                GroupMaterialCode = source.GroupMaterialCode,
                IsMixedMaterial = source.IsMixedMaterial,
                HasActiveTask = source.HasActiveTask
            };
        }
    }
}
