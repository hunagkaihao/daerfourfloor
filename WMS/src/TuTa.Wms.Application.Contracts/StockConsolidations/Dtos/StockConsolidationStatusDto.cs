using System;

namespace TuTa.Wms.StockConsolidations.Dtos
{
    /// <summary>
    /// 四楼库存整理线程状态。
    /// </summary>
    public class StockConsolidationStatusDto
    {
        /// <summary>
        /// 配置文件是否已经启用库存整理功能。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 后台整理线程是否仍在运行。
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 是否已经收到停止请求，正在等待当前搬运任务结束。
        /// </summary>
        public bool IsStopping { get; set; }

        /// <summary>
        /// 当前状态文本，例如运行中、正在停止、已完成或异常停止。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 本次整理线程启动时间。
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 本次整理线程结束时间。
        /// </summary>
        public DateTime? StoppedAt { get; set; }

        /// <summary>
        /// 当前S型遍历到的库位。
        /// </summary>
        public string CurrentCellCode { get; set; }

        /// <summary>
        /// 当前正在整理的主物料编码。
        /// </summary>
        public string CurrentMaterialCode { get; set; }

        /// <summary>
        /// 当前动作，例如腾位、归拢或暂存物料回收。
        /// </summary>
        public string CurrentAction { get; set; }

        /// <summary>
        /// 当前搬运起点。
        /// </summary>
        public string CurrentFromCell { get; set; }

        /// <summary>
        /// 当前搬运终点。
        /// </summary>
        public string CurrentToCell { get; set; }

        /// <summary>
        /// 已完成整理的物料组数量。
        /// </summary>
        public int CompletedGroupCount { get; set; }

        /// <summary>
        /// 已完成的搬运任务数量。
        /// </summary>
        public int CompletedMoveCount { get; set; }

        /// <summary>
        /// 最近一次异常信息。
        /// </summary>
        public string LastError { get; set; }
    }
}
