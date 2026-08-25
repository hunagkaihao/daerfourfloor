using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Planning;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Planning;

public sealed class ConsolidationPlanner(
    SShapeCellOrderBuilder orderBuilder,
    IOptions<ConsolidationOptions> options)
{
    private readonly ConsolidationOptions _options = options.Value;

    public ConsolidationPlanDraft CreatePlan(WarehouseSnapshot snapshot)
    {
        var orderedPositions = orderBuilder.Build(snapshot);
        var orderedCells = orderedPositions.Select(position => position.CellCode).ToList();
        var managedCells = orderedCells
            .Concat(_options.BufferCells)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bufferCells = _options.BufferCells
            .Where(snapshot.Cells.ContainsKey)
            .Where(cellCode => snapshot.Cells[cellCode].IsEnabled)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inconsistentCells = managedCells
            .Where(snapshot.Cells.ContainsKey)
            .Where(cellCode =>
            {
                var cell = snapshot.Cells[cellCode];
                var reportsEmpty = string.Equals(cell.CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase);
                return reportsEmpty != cell.IsEmpty;
            })
            .ToList();
        if (inconsistentCells.Count > 0)
        {
            throw new ConsolidationPlanningException(
                $"以下库位的 CellStatus 与库存快照不一致：{string.Join("、", inconsistentCells)}。");
        }

        if (bufferCells.Count == 0)
        {
            throw new ConsolidationPlanningException("配置的 4B 周转库位在 WMS 中不存在或不可用。");
        }

        var pallets = snapshot.Pallets.Values
            .Where(pallet => managedCells.Contains(pallet.CurrentCellCode))
            .ToDictionary(pallet => pallet.PalletKey, ClonePallet, StringComparer.OrdinalIgnoreCase);
        var occupancy = managedCells.ToDictionary(
            cellCode => cellCode,
            cellCode => snapshot.Cells.TryGetValue(cellCode, out var state) ? state.PalletKey : null,
            StringComparer.OrdinalIgnoreCase);

        var emptyBuffers = bufferCells.Count(cellCode =>
            occupancy[cellCode] is null &&
            string.Equals(snapshot.Cells[cellCode].CellStatus, "Nohave", StringComparison.OrdinalIgnoreCase));
        if (emptyBuffers < _options.MinimumEmptyBufferCells)
        {
            throw new ConsolidationPlanningException(
                $"4B 周转区至少需要 {_options.MinimumEmptyBufferCells} 个空位，当前只有 {emptyBuffers} 个。");
        }

        var groups = new List<PlannedGroup>();
        var warnings = new List<string>();
        var finalizedCells = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var finalizedPallets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cursor = 0;
        var moveSequence = 1;
        var currentHole = FindEmptyCell(bufferCells, occupancy)
                          ?? FindEmptyCell(orderedCells, occupancy)
                          ?? throw new ConsolidationPlanningException("没有可供整理启动的空库位。");

        while (cursor < orderedCells.Count)
        {
            var remainingPallets = pallets.Values
                .Where(pallet => !finalizedPallets.Contains(pallet.PalletKey))
                .Where(pallet => managedCells.Contains(pallet.CurrentCellCode))
                .ToList();
            if (remainingPallets.Count == 0)
            {
                break;
            }

            var cursorCell = orderedCells[cursor];
            PalletSnapshot? seedPallet = null;
            if (occupancy[cursorCell] is { } cursorPalletKey)
            {
                seedPallet = pallets[cursorPalletKey];
            }

            seedPallet ??= remainingPallets
                .Where(pallet => !bufferCells.Contains(pallet.CurrentCellCode))
                .OrderBy(pallet => IndexOfCell(orderedCells, pallet.CurrentCellCode))
                .FirstOrDefault();
            seedPallet ??= remainingPallets
                .OrderBy(pallet => pallet.CurrentCellCode, StringComparer.OrdinalIgnoreCase)
                .First();

            var groupPallets = remainingPallets
                .Where(pallet => string.Equals(
                    pallet.GroupBarcode,
                    seedPallet.GroupBarcode,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (groupPallets.Any(pallet => pallet.HasActiveTask))
            {
                throw new ConsolidationPlanningException(
                    $"物料组 {seedPallet.GroupBarcode} 中存在已有活动任务的托盘，不能开始整理。");
            }

            if (cursor + groupPallets.Count > orderedCells.Count)
            {
                throw new ConsolidationPlanningException(
                    $"物料组 {seedPallet.GroupBarcode} 需要 {groupPallets.Count} 个库位，但 S 型序列剩余容量不足。");
            }

            var targetCells = orderedCells.Skip(cursor).Take(groupPallets.Count).ToList();
            if (!_options.AllowCrossRow && CrossesRow(targetCells, orderedPositions))
            {
                throw new ConsolidationPlanningException(
                    $"物料组 {seedPallet.GroupBarcode} 的目标块需要跨排，但配置禁止跨排。");
            }

            var moves = new List<PlannedMove>();

            foreach (var targetCell in targetCells.Where(cellCode => occupancy[cellCode] is null).ToList())
            {
                var source = FindGroupSource(groupPallets, targetCells, bufferCells);
                if (source is null)
                {
                    throw new ConsolidationPlanningException(
                        $"物料组 {seedPallet.GroupBarcode} 缺少可搬入空目标位 {targetCell} 的来源托盘。");
                }

                var sourceCell = source.CurrentCellCode;
                AddMove(
                    moves,
                    source,
                    sourceCell,
                    targetCell,
                    bufferCells.Contains(sourceCell)
                        ? ConsolidationMoveType.ReturnFromBuffer
                        : ConsolidationMoveType.Consolidate,
                    ref moveSequence);
                ApplyMove(source, sourceCell, targetCell, occupancy);
                currentHole = sourceCell;
            }

            foreach (var targetCell in targetCells.ToList())
            {
                var targetPalletKey = occupancy[targetCell];
                if (targetPalletKey is null)
                {
                    continue;
                }

                var targetPallet = pallets[targetPalletKey];
                if (string.Equals(targetPallet.GroupBarcode, seedPallet.GroupBarcode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (targetPallet.HasActiveTask)
                {
                    throw new ConsolidationPlanningException(
                        $"目标位 {targetCell} 的阻挡托盘存在活动任务，不能腾位。");
                }

                if (!IsAvailableHole(currentHole, occupancy, finalizedCells, targetCells))
                {
                    currentHole = FindEmptyCell(bufferCells, occupancy)
                                  ?? FindEmptyFutureCell(orderedCells, cursor, targetCells, finalizedCells, occupancy)
                                  ?? throw new ConsolidationPlanningException(
                                      $"整理物料组 {seedPallet.GroupBarcode} 时没有可用周转空位。");
                }

                var blockerFrom = targetPallet.CurrentCellCode;
                var blockerTo = currentHole;
                AddMove(
                    moves,
                    targetPallet,
                    blockerFrom,
                    blockerTo,
                    bufferCells.Contains(blockerTo)
                        ? ConsolidationMoveType.EvictToBuffer
                        : ConsolidationMoveType.RelocateBlocker,
                    ref moveSequence);
                ApplyMove(targetPallet, blockerFrom, blockerTo, occupancy);

                var source = FindGroupSource(groupPallets, targetCells, bufferCells)
                             ?? throw new ConsolidationPlanningException(
                                 $"目标位 {targetCell} 已腾空，但没有物料组 {seedPallet.GroupBarcode} 的来源托盘。");
                var sourceCell = source.CurrentCellCode;
                AddMove(
                    moves,
                    source,
                    sourceCell,
                    targetCell,
                    bufferCells.Contains(sourceCell)
                        ? ConsolidationMoveType.ReturnFromBuffer
                        : ConsolidationMoveType.Consolidate,
                    ref moveSequence);
                ApplyMove(source, sourceCell, targetCell, occupancy);
                currentHole = sourceCell;
            }

            var invalidTarget = targetCells.FirstOrDefault(cellCode =>
                occupancy[cellCode] is not { } palletKey ||
                !string.Equals(
                    pallets[palletKey].GroupBarcode,
                    seedPallet.GroupBarcode,
                    StringComparison.OrdinalIgnoreCase));
            if (invalidTarget is not null)
            {
                throw new ConsolidationPlanningException(
                    $"物料组 {seedPallet.GroupBarcode} 仿真后目标位 {invalidTarget} 不是预期物料。");
            }

            foreach (var targetCell in targetCells)
            {
                finalizedCells.Add(targetCell);
                if (occupancy[targetCell] is { } palletKey)
                {
                    finalizedPallets.Add(palletKey);
                }
            }

            groups.Add(new PlannedGroup
            {
                Sequence = groups.Count + 1,
                GroupBarcode = seedPallet.GroupBarcode,
                TargetCells = targetCells,
                Moves = moves
            });
            cursor += groupPallets.Count;
        }

        var occupiedBuffers = bufferCells.Where(cellCode => occupancy[cellCode] is not null).ToList();
        if (occupiedBuffers.Count > 0)
        {
            warnings.Add($"规划结束后 4B 仍有物料：{string.Join("、", occupiedBuffers)}。执行前需复核分组和容量。");
        }

        return new ConsolidationPlanDraft
        {
            SnapshotTimeUtc = snapshot.CapturedAtUtc,
            OrderedUsableCells = orderedCells,
            Groups = groups,
            CurrentHole = currentHole,
            FinalCursorIndex = cursor,
            Warnings = warnings
        };
    }

    private static PalletSnapshot ClonePallet(PalletSnapshot source) => new()
    {
        PalletKey = source.PalletKey,
        CurrentBoxCode = source.CurrentBoxCode,
        CurrentCellCode = source.CurrentCellCode,
        StockIds = [.. source.StockIds],
        Barcodes = [.. source.Barcodes],
        GroupBarcode = source.GroupBarcode,
        MaterialCode = source.MaterialCode,
        HasActiveTask = source.HasActiveTask
    };

    private static PalletSnapshot? FindGroupSource(
        IEnumerable<PalletSnapshot> groupPallets,
        IReadOnlyCollection<string> targetCells,
        IReadOnlySet<string> bufferCells) =>
        groupPallets
            .Where(pallet => !targetCells.Contains(pallet.CurrentCellCode, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(pallet => bufferCells.Contains(pallet.CurrentCellCode))
            .ThenBy(pallet => pallet.CurrentCellCode, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

    private static void AddMove(
        ICollection<PlannedMove> moves,
        PalletSnapshot pallet,
        string fromCell,
        string toCell,
        ConsolidationMoveType moveType,
        ref int sequence)
    {
        moves.Add(new PlannedMove
        {
            Sequence = sequence++,
            PalletKey = pallet.PalletKey,
            StockIds = [.. pallet.StockIds],
            GroupBarcode = pallet.GroupBarcode,
            FromCell = fromCell,
            ToCell = toCell,
            MoveType = moveType
        });
    }

    private static void ApplyMove(
        PalletSnapshot pallet,
        string fromCell,
        string toCell,
        IDictionary<string, string?> occupancy)
    {
        if (!string.Equals(occupancy[fromCell], pallet.PalletKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConsolidationPlanningException($"托盘 {pallet.PalletKey} 不在计划起点 {fromCell}。");
        }

        if (occupancy[toCell] is not null)
        {
            throw new ConsolidationPlanningException($"计划目标位 {toCell} 不是空位。");
        }

        occupancy[fromCell] = null;
        occupancy[toCell] = pallet.PalletKey;
        pallet.CurrentCellCode = toCell;
    }

    private static bool IsAvailableHole(
        string? hole,
        IReadOnlyDictionary<string, string?> occupancy,
        IReadOnlySet<string> finalizedCells,
        IReadOnlyCollection<string> currentTargets) =>
        !string.IsNullOrWhiteSpace(hole) &&
        occupancy.TryGetValue(hole, out var occupant) &&
        occupant is null &&
        !finalizedCells.Contains(hole) &&
        !currentTargets.Contains(hole, StringComparer.OrdinalIgnoreCase);

    private static string? FindEmptyCell(
        IEnumerable<string> cells,
        IReadOnlyDictionary<string, string?> occupancy) =>
        cells.FirstOrDefault(cell => occupancy.TryGetValue(cell, out var palletKey) && palletKey is null);

    private static string? FindEmptyFutureCell(
        IReadOnlyList<string> orderedCells,
        int cursor,
        IReadOnlyCollection<string> targetCells,
        IReadOnlySet<string> finalizedCells,
        IReadOnlyDictionary<string, string?> occupancy) =>
        orderedCells
            .Skip(cursor)
            .FirstOrDefault(cell =>
                !targetCells.Contains(cell, StringComparer.OrdinalIgnoreCase) &&
                !finalizedCells.Contains(cell) &&
                occupancy[cell] is null);

    private static int IndexOfCell(IReadOnlyList<string> cells, string cellCode)
    {
        for (var index = 0; index < cells.Count; index++)
        {
            if (string.Equals(cells[index], cellCode, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    private static bool CrossesRow(
        IReadOnlyList<string> targetCells,
        IReadOnlyList<CellPosition> positions)
    {
        var rowByCell = positions.ToDictionary(position => position.CellCode, position => position.Row, StringComparer.OrdinalIgnoreCase);
        return targetCells.Select(cell => rowByCell[cell]).Distinct().Skip(1).Any();
    }
}

public sealed class ConsolidationPlanningException(string message) : Exception(message);
