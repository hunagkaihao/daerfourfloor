using System;
using System.Collections.Generic;
using TuTa.Wms.AgvTasks;

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
        public string RunStatus { get; set; }
        public string ContainerKey { get; set; }

        /// <summary>
        /// 是否为空位。业务规则只看库位是否绑定容器，不依据库存条数或CellStatus判断。
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(ContainerKey);
    }

    /// <summary>
    /// 整理算法使用的容器快照。
    /// 一个库位最多绑定一个容器，一个容器允许包含多条库存和多个物料。
    /// </summary>
    internal class StockConsolidationContainerSnapshot
    {
        public string ContainerKey { get; set; }
        public string BoxCode { get; set; }
        public string CellCode { get; set; }
        public List<Guid> StockIds { get; set; } = new List<Guid>();
        public List<string> Barcodes { get; set; } = new List<string>();
        /// <summary>
        /// 容器内数量最多的物料编码；数量相同时取查询顺序最先出现的物料。
        /// </summary>
        public string GroupMaterialCode { get; set; }
        public bool IsMixedMaterial { get; set; }
        public bool HasActiveTask { get; set; }
    }

    /// <summary>
    /// 一次仓库查询形成的不可变业务快照。
    /// </summary>
    internal class StockConsolidationSnapshot
    {
        public Dictionary<string, StockConsolidationCellSnapshot> Cells { get; set; }
            = new Dictionary<string, StockConsolidationCellSnapshot>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, StockConsolidationContainerSnapshot> Containers { get; set; }
            = new Dictionary<string, StockConsolidationContainerSnapshot>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 一条库存整理搬运计划。
    /// </summary>
    internal class StockConsolidationMovePlan
    {
        public int Sequence { get; set; }
        public string ContainerKey { get; set; }
        public List<Guid> StockIds { get; set; } = new List<Guid>();
        public string GroupMaterialCode { get; set; }
        public string FromCell { get; set; }
        public string ToCell { get; set; }
        public string MoveType { get; set; }
    }

    /// <summary>
    /// 当前同物料组的整理计划。
    /// </summary>
    internal class StockConsolidationGroupPlan
    {
        /// <summary>
        /// 规划是否成功。失败时Worker打印ErrorMessage并安全结束线程。
        /// </summary>
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
        public string GroupMaterialCode { get; set; }
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
        public string CurrentMaterialCode { get; set; }
        public string CurrentAction { get; set; }
        public string CurrentFromCell { get; set; }
        public string CurrentToCell { get; set; }
        public int CompletedGroupCount { get; set; }
        public int CompletedMoveCount { get; set; }
        public string LastError { get; set; }
    }

    /// <summary>
    /// AGV任务查询结果快照。
    /// 只携带库存整理需要的标量，避免在UnitOfWork结束后继续持有实体。
    /// </summary>
    internal class StockConsolidationAgvTaskSnapshot
    {
        public string ReqCode { get; set; }
        public AgvTaskStatus Status { get; set; }
    }

    /// <summary>
    /// 仓库快照构建结果，使用结果对象传递错误而不是抛出异常。
    /// </summary>
    internal class StockConsolidationSnapshotResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public StockConsolidationSnapshot Snapshot { get; set; }
    }

    /// <summary>
    /// 单条搬运执行结果。
    /// </summary>
    internal class StockConsolidationMoveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// AGV任务查询结果，区分“暂未查到”和“查询失败”。
    /// </summary>
    internal class StockConsolidationAgvQueryResult
    {
        public bool IsSuccess { get; set; }
        public bool IsFound { get; set; }
        public string ErrorMessage { get; set; }
        public StockConsolidationAgvTaskSnapshot Task { get; set; }
    }
}
