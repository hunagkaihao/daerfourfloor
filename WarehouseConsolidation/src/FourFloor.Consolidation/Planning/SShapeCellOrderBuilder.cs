using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Planning;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Planning;

public sealed class SShapeCellOrderBuilder(
    CellCodeParser parser,
    IOptions<ConsolidationOptions> options)
{
    private readonly ConsolidationOptions _options = options.Value;

    public IReadOnlyList<CellPosition> Build(WarehouseSnapshot snapshot)
    {
        var configuredRows = _options.Rows.ToHashSet();
        var layerPriority = _options.LayerOrder
            .Select((layer, index) => (layer, index))
            .ToDictionary(item => item.layer, item => item.index);

        var positions = snapshot.Cells.Values
            .Where(cell => cell.IsEnabled)
            .Select(cell => parser.TryParse4F(cell.CellCode, out var position) ? position : null)
            .Where(position => position is not null)
            .Cast<CellPosition>()
            .Where(position => configuredRows.Contains(position.Row))
            .Where(position => layerPriority.ContainsKey(position.Layer))
            .Where(position => !IsExcluded(position))
            .ToList();

        var result = new List<CellPosition>();
        foreach (var row in _options.Rows)
        {
            var rowPositions = positions.Where(position => position.Row == row);
            rowPositions = row % 2 == 0
                ? rowPositions.OrderByDescending(position => position.Column)
                    .ThenBy(position => layerPriority[position.Layer])
                : rowPositions.OrderBy(position => position.Column)
                    .ThenBy(position => layerPriority[position.Layer]);

            foreach (var position in rowPositions)
            {
                result.Add(position with { SequenceIndex = result.Count });
            }
        }

        return result;
    }

    private bool IsExcluded(CellPosition position)
    {
        foreach (var range in _options.ExcludedRanges)
        {
            if (!parser.TryParse4F(range.From, out var from) || !parser.TryParse4F(range.To, out var to))
            {
                continue;
            }

            if (from.Row == to.Row && position.Row == from.Row)
            {
                var minColumn = Math.Min(from.Column, to.Column);
                var maxColumn = Math.Max(from.Column, to.Column);
                if (position.Column >= minColumn && position.Column <= maxColumn)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
