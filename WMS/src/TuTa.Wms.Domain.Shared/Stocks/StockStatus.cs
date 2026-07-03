namespace TuTa.Wms.Stocks
{
    public enum StockStatus
    {
        /// <summary>
        /// 可用的
        /// </summary>
        Available = 1,
        /// <summary>
        /// 锁定的（如AGV准备取料时）
        /// </summary>
        Locked,
        /// <summary>
        /// 冻结的
        /// </summary>
        Freezing,
        /// <summary>
        /// 待入库
        /// </summary>
        Waiting,
        /// <summary>
        /// 发送车间
        /// </summary>
        StockOut,
        /// <summary>
        /// 筛选
        /// </summary>
        Filtrate
    }
}
