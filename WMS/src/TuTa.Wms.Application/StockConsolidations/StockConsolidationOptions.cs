using System.Collections.Generic;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理配置。
    /// 配置来源为HttpApi.Host的appsettings.json中StockConsolidation节点。
    /// </summary>
    public class StockConsolidationOptions
    {
        /// <summary>
        /// 是否允许启动库存整理线程。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 是否启用每日自动整理。
        /// 关闭后只禁止定时触发，移动端的手动启动、停止按钮仍然保留并继续生效。
        /// </summary>
        public bool AutoStartEnabled { get; set; }

        /// <summary>
        /// 每日自动启动时间，使用WMS服务器本地时间和24小时制HH:mm格式。
        /// 默认晚上22点；服务在当天配置时间之后才启动时不补跑，避免重启后突然下发搬运任务。
        /// </summary>
        public string DailyStartTime { get; set; } = "22:00";

        /// <summary>
        /// 参与整理的排号，顺序同时也是遍历顺序。
        /// </summary>
        public List<int> Rows { get; set; } = new List<int> { 12, 11, 10, 9, 8, 7, 6, 5, 4 };

        /// <summary>
        /// 同一列的层遍历顺序，当前业务为先二层再一层。
        /// </summary>
        public List<int> LayerOrder { get; set; } = new List<int> { 2, 1 };

        /// <summary>
        /// 是否允许同一物料目标块跨排。
        /// </summary>
        public bool AllowCrossRow { get; set; } = true;

        /// <summary>
        /// 不参与整理的库位范围。
        /// </summary>
        public List<StockConsolidationCellRange> ExcludedRanges { get; set; } = new List<StockConsolidationCellRange>();

        /// <summary>
        /// 允许作为临时周转位的4B库位。
        /// </summary>
        public List<string> BufferCells { get; set; } = new List<string>();

        /// <summary>
        /// 启动时4B至少需要保留的空库位数。
        /// </summary>
        public int MinimumEmptyBufferCells { get; set; } = 1;

        /// <summary>
        /// AGV任务状态轮询间隔秒数。
        /// </summary>
        public int PollIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// 单条AGV任务最长等待分钟数。
        /// </summary>
        public int TaskTimeoutMinutes { get; set; } = 60;
    }

    /// <summary>
    /// 库位排除范围配置。
    /// 同一排内按列范围排除，两个层都会被排除。
    /// </summary>
    public class StockConsolidationCellRange
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}
