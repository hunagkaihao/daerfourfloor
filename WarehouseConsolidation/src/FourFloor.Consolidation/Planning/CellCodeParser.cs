using System.Globalization;
using FourFloor.Consolidation.Models.Planning;

namespace FourFloor.Consolidation.Planning;

public sealed class CellCodeParser
{
    public bool TryParse4F(string? cellCode, out CellPosition position)
    {
        position = default!;
        if (string.IsNullOrWhiteSpace(cellCode) ||
            !cellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
            cellCode.Length != 8)
        {
            return false;
        }

        if (!int.TryParse(cellCode.AsSpan(2, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var row) ||
            !int.TryParse(cellCode.AsSpan(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var column) ||
            !int.TryParse(cellCode.AsSpan(6, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var layer))
        {
            return false;
        }

        position = new CellPosition(cellCode.ToUpperInvariant(), row, column, layer, -1);
        return true;
    }
}
