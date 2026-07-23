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

    public enum InspectionStatus
    {
        /// <summary>
        /// 等待抽检
        /// </summary>
        AwaitingInspection,
        /// <summary>
        /// 抽检中
        /// </summary>
        InProgressInspection,
        /// <summary>
        /// 抽检合格
        /// </summary>
        InspectionQualified,
        /// <summary>
        /// 抽检不合格
        /// </summary>
        InspectionNotQualified,
        /// <summary>
        /// 抽检完成
        /// </summary>
        InspectionCompleted
    }
}
