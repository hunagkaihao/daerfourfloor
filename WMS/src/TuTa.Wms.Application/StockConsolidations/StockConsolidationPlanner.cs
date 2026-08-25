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
                .ToList();
            var managedCells = new HashSet<string>(orderedCells, StringComparer.OrdinalIgnoreCase);
            managedCells.UnionWith(validBufferCells);
            var pallets = snapshot.Pallets.Values
                .Where(pallet => managedCells.Contains(pallet.CellCode))
                .ToDictionary(pallet => pallet.PalletKey, ClonePallet, StringComparer.OrdinalIgnoreCase);
            if (pallets.Count == 0)
            {
                return null;
            }

            var occupancy = managedCells.ToDictionary(
                cellCode => cellCode,
                cellCode => snapshot.Cells.TryGetValue(cellCode, out var cell) ? cell.PalletKey : null,
                StringComparer.OrdinalIgnoreCase);

            var cursorCell = orderedCells[cursorIndex];
            StockConsolidationPalletSnapshot seedPallet = null;
            if (occupancy.TryGetValue(cursorCell, out var cursorPalletKey) &&
                !string.IsNullOrWhiteSpace(cursorPalletKey))
            {
                seedPallet = pallets[cursorPalletKey];
            }

            // 当前游标为空时，从后续S型库位寻找第一托货物填补空位。
            seedPallet ??= pallets.Values
                .Where(pallet => orderedCells.Skip(cursorIndex).Contains(pallet.CellCode, StringComparer.OrdinalIgnoreCase))
                .OrderBy(pallet => IndexOf(orderedCells, pallet.CellCode))
                .FirstOrDefault();
            seedPallet ??= pallets.Values
                .Where(pallet => validBufferCells.Contains(pallet.CellCode, StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (seedPallet == null)
            {
                return null;
            }

            var groupPallets = pallets.Values
                .Where(pallet => string.Equals(pallet.GroupBarcode, seedPallet.GroupBarcode, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (groupPallets.Any(pallet => pallet.HasActiveTask))
            {
                throw new InvalidOperationException($"物料组{seedPallet.GroupBarcode}存在活动任务，无法整理。");
            }

            if (cursorIndex + groupPallets.Count > orderedCells.Count)
            {
                throw new InvalidOperationException($"物料组{seedPallet.GroupBarcode}所需库位超过剩余可用容量。");
            }

            var targetCells = orderedCells.Skip(cursorIndex).Take(groupPallets.Count).ToList();
            var moves = new List<StockConsolidationMovePlan>();
            var moveSequence = 1;

            // 先填充目标块中已经存在的空位，以产生后续腾位所需的新空洞。
            foreach (var targetCell in targetCells.Where(cellCode => string.IsNullOrWhiteSpace(occupancy[cellCode])).ToList())
            {
                var source = FindSource(groupPallets, targetCells, validBufferCells);
                if (source == null)
                {
                    throw new InvalidOperationException($"目标位{targetCell}为空，但找不到物料组{seedPallet.GroupBarcode}的来源托盘。");
                }

                var sourceCell = source.CellCode;
                AddMove(moves, source, sourceCell, targetCell,
                    validBufferCells.Contains(sourceCell, StringComparer.OrdinalIgnoreCase) ? "暂存物料回收" : "归拢",
                    moveSequence++);
                ApplyMove(source, sourceCell, targetCell, occupancy);
                currentHole = sourceCell;
            }

            // 对目标块中的异物执行“异物到空洞、目标物料到异物原位”的交替搬运。
            foreach (var targetCell in targetCells)
            {
                var targetPalletKey = occupancy[targetCell];
                if (string.IsNullOrWhiteSpace(targetPalletKey))
                {
                    continue;
                }

                var blocker = pallets[targetPalletKey];
                if (string.Equals(blocker.GroupBarcode, seedPallet.GroupBarcode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (blocker.HasActiveTask)
                {
                    throw new InvalidOperationException($"目标位{targetCell}的阻挡托盘存在活动任务。");
                }

                if (!IsUsableHole(currentHole, occupancy, targetCells, orderedCells.Take(cursorIndex)))
                {
                    currentHole = FindEmptyCell(validBufferCells, occupancy)
                                  ?? FindEmptyCell(orderedCells.Skip(cursorIndex), occupancy)
                                  ?? throw new InvalidOperationException("没有可用于腾位的空库位。");
                }

                var blockerFrom = blocker.CellCode;
                var blockerTo = currentHole;
                AddMove(moves, blocker, blockerFrom, blockerTo,
                    validBufferCells.Contains(blockerTo, StringComparer.OrdinalIgnoreCase) ? "腾位到4B" : "腾位",
                    moveSequence++);
                ApplyMove(blocker, blockerFrom, blockerTo, occupancy);

                var source = FindSource(groupPallets, targetCells, validBufferCells)
                             ?? throw new InvalidOperationException($"目标位{targetCell}已腾空，但找不到对应目标物料。");
                var sourceCell = source.CellCode;
                AddMove(moves, source, sourceCell, targetCell,
                    validBufferCells.Contains(sourceCell, StringComparer.OrdinalIgnoreCase) ? "暂存物料回收" : "归拢",
                    moveSequence++);
                ApplyMove(source, sourceCell, targetCell, occupancy);
                currentHole = sourceCell;
            }

            return new StockConsolidationGroupPlan
            {
                GroupBarcode = seedPallet.GroupBarcode,
                TargetCells = targetCells,
                Moves = moves,
                NextCursorIndex = cursorIndex + groupPallets.Count,
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

        private static StockConsolidationPalletSnapshot FindSource(
            IEnumerable<StockConsolidationPalletSnapshot> groupPallets,
            IEnumerable<string> targetCells,
            IEnumerable<string> bufferCells)
        {
            var targetSet = new HashSet<string>(targetCells, StringComparer.OrdinalIgnoreCase);
            var bufferSet = new HashSet<string>(bufferCells, StringComparer.OrdinalIgnoreCase);
            return groupPallets
                .Where(pallet => !targetSet.Contains(pallet.CellCode))
                .OrderByDescending(pallet => bufferSet.Contains(pallet.CellCode))
                .ThenBy(pallet => pallet.CellCode)
                .FirstOrDefault();
        }

        private static void AddMove(
            ICollection<StockConsolidationMovePlan> moves,
            StockConsolidationPalletSnapshot pallet,
            string fromCell,
            string toCell,
            string moveType,
            int sequence)
        {
            moves.Add(new StockConsolidationMovePlan
            {
                Sequence = sequence,
                PalletKey = pallet.PalletKey,
                StockIds = pallet.StockIds.ToList(),
                GroupBarcode = pallet.GroupBarcode,
                FromCell = fromCell,
                ToCell = toCell,
                MoveType = moveType
            });
        }

        private static void ApplyMove(
            StockConsolidationPalletSnapshot pallet,
            string fromCell,
            string toCell,
            IDictionary<string, string> occupancy)
        {
            if (!occupancy.TryGetValue(fromCell, out var currentPallet) ||
                !string.Equals(currentPallet, pallet.PalletKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"托盘{pallet.PalletKey}不在计划起点{fromCell}。");
            }

            if (occupancy.TryGetValue(toCell, out var targetPallet) && !string.IsNullOrWhiteSpace(targetPallet))
            {
                throw new InvalidOperationException($"计划目标位{toCell}不是空位。");
            }

            occupancy[fromCell] = null;
            occupancy[toCell] = pallet.PalletKey;
            pallet.CellCode = toCell;
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

        private static StockConsolidationPalletSnapshot ClonePallet(StockConsolidationPalletSnapshot source)
        {
            return new StockConsolidationPalletSnapshot
            {
                PalletKey = source.PalletKey,
                BoxCode = source.BoxCode,
                CellCode = source.CellCode,
                StockIds = source.StockIds.ToList(),
                Barcodes = source.Barcodes.ToList(),
                GroupBarcode = source.GroupBarcode,
                HasActiveTask = source.HasActiveTask
            };
        }
    }
}
