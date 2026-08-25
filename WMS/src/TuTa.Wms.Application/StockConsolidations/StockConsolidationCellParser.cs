using System;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库位编码解析器。
    /// 编码格式为4F + 两位排 + 两位列 + 两位层，例如4F128002。
    /// </summary>
    internal class StockConsolidationCellParser
    {
        /// <summary>
        /// 尝试解析四楼库位编码。
        /// </summary>
        public bool TryParse(string cellCode, out StockConsolidationCellPosition position)
        {
            position = null;
            if (string.IsNullOrWhiteSpace(cellCode) ||
                !cellCode.StartsWith("4F", StringComparison.OrdinalIgnoreCase) ||
                cellCode.Length != 8)
            {
                return false;
            }

            if (!int.TryParse(cellCode.Substring(2, 2), out var row) ||
                !int.TryParse(cellCode.Substring(4, 2), out var column) ||
                !int.TryParse(cellCode.Substring(6, 2), out var layer))
            {
                return false;
            }

            position = new StockConsolidationCellPosition
            {
                CellCode = cellCode.ToUpperInvariant(),
                Row = row,
                Column = column,
                Layer = layer
            };
            return true;
        }
    }
}
