using System;
using System.Collections.Generic;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 解析后的四楼库位坐标。
    /// </summary>
    internal class StockConsolidationCellPosition
    {
        public string CellCode { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Layer { get; set; }
    }

    /// <summary>
    /// 整理算法使用的库位快照。
    /// </summary>
    internal class StockConsolidationCellSnapshot
    {
        public string CellCode { get; set; }
        public string CellStatus { get; set; }
        public string RunStatus { get; set; }
        public string PalletKey { get; set; }

        /// <summary>
        /// 是否为空位。库存与库位状态一致性由Worker在生成快照时校验。
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(PalletKey);
    }

    /// <summary>
    /// 整理算法使用的托盘快照。
    /// StockId组合用于在搬运后重新定位当前容器。
    /// </summary>
    internal class StockConsolidationPalletSnapshot
    {
        public string PalletKey { get; set; }
        public string BoxCode { get; set; }
        public string CellCode { get; set; }
        public List<Guid> StockIds { get; set; } = new List<Guid>();
        public List<string> Barcodes { get; set; } = new List<string>();
        public string GroupBarcode { get; set; }
        public bool HasActiveTask { get; set; }
    }

    /// <summary>
    /// 一次仓库查询形成的不可变业务快照。
    /// </summary>
    internal class StockConsolidationSnapshot
    {
        public Dictionary<string, StockConsolidationCellSnapshot> Cells { get; set; }
            = new Dictionary<string, StockConsolidationCellSnapshot>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, StockConsolidationPalletSnapshot> Pallets { get; set; }
            = new Dictionary<string, StockConsolidationPalletSnapshot>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 一条库存整理搬运计划。
    /// </summary>
    internal class StockConsolidationMovePlan
    {
        public int Sequence { get; set; }
        public string PalletKey { get; set; }
        public List<Guid> StockIds { get; set; } = new List<Guid>();
        public string GroupBarcode { get; set; }
        public string FromCell { get; set; }
        public string ToCell { get; set; }
        public string MoveType { get; set; }
    }

    /// <summary>
    /// 当前同物料组的整理计划。
    /// </summary>
    internal class StockConsolidationGroupPlan
    {
        public string GroupBarcode { get; set; }
        public List<string> TargetCells { get; set; } = new List<string>();
        public List<StockConsolidationMovePlan> Moves { get; set; } = new List<StockConsolidationMovePlan>();
        public int NextCursorIndex { get; set; }
        public string NextHoleCell { get; set; }
    }

    /// <summary>
    /// Worker向单例调度服务上报的实时进度。
    /// </summary>
    internal class StockConsolidationProgress
    {
        public string Status { get; set; }
        public string CurrentCellCode { get; set; }
        public string CurrentGroupBarcode { get; set; }
        public string CurrentAction { get; set; }
        public string CurrentFromCell { get; set; }
        public string CurrentToCell { get; set; }
        public int CompletedGroupCount { get; set; }
        public int CompletedMoveCount { get; set; }
        public string LastError { get; set; }
    }
}
