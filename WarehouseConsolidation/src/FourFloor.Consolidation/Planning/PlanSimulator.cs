using FourFloor.Consolidation.Models.Planning;

namespace FourFloor.Consolidation.Planning;

public sealed class PlanSimulator
{
    public PlanSimulationResult Validate(WarehouseSnapshot snapshot, ConsolidationPlanDraft plan)
    {
        var occupancy = snapshot.Cells.Values.ToDictionary(
            cell => cell.CellCode,
            cell => cell.PalletKey,
            StringComparer.OrdinalIgnoreCase);
        var palletCells = snapshot.Pallets.Values.ToDictionary(
            pallet => pallet.PalletKey,
            pallet => pallet.CurrentCellCode,
            StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var move in plan.Groups.SelectMany(group => group.Moves).OrderBy(move => move.Sequence))
        {
            if (!occupancy.TryGetValue(move.FromCell, out var fromPallet) ||
                !string.Equals(fromPallet, move.PalletKey, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"任务 {move.Sequence} 起点 {move.FromCell} 不包含托盘 {move.PalletKey}。");
                continue;
            }

            if (!occupancy.TryGetValue(move.ToCell, out var toPallet))
            {
                errors.Add($"任务 {move.Sequence} 目标库位 {move.ToCell} 不存在于快照。");
                continue;
            }

            if (toPallet is not null)
            {
                errors.Add($"任务 {move.Sequence} 目标库位 {move.ToCell} 已被托盘 {toPallet} 占用。");
                continue;
            }

            occupancy[move.FromCell] = null;
            occupancy[move.ToCell] = move.PalletKey;
            palletCells[move.PalletKey] = move.ToCell;
        }

        foreach (var group in plan.Groups)
        {
            foreach (var targetCell in group.TargetCells)
            {
                if (!occupancy.TryGetValue(targetCell, out var palletKey) || palletKey is null)
                {
                    errors.Add($"物料组 {group.GroupBarcode} 的目标位 {targetCell} 最终为空。");
                    continue;
                }

                if (!snapshot.Pallets.TryGetValue(palletKey, out var originalPallet) ||
                    !string.Equals(originalPallet.GroupBarcode, group.GroupBarcode, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"物料组 {group.GroupBarcode} 的目标位 {targetCell} 最终物料不一致。");
                }
            }
        }

        return new PlanSimulationResult(errors.Count == 0, errors);
    }
}

public sealed record PlanSimulationResult(bool IsValid, IReadOnlyList<string> Errors);
