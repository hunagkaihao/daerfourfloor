using System;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.Stocks;

namespace TuTa.Wms.AgvTasks.Dtos
{
    /// <summary>
    /// AGV任务分页查询参数
    /// </summary>
    public class AgvTaskPagedQueryDto
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 跳过数量
        /// </summary>
        public int SkipCount => (PageIndex - 1) * PageSize;

        /// <summary>
        /// 最大结果数量
        /// </summary>
        public int MaxResultCount => PageSize;

        /// <summary>
        /// 任务请求编号
        /// </summary>
        public string ReqCode { get; set; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string TaskTyp { get; set; }

        /// <summary>
        /// 物料任务类型
        /// </summary>
        public ManageType? StockTyp { get; set; }

        /// <summary>
        /// 工作位编码
        /// </summary>
        public string WbCode { get; set; }

        /// <summary>
        /// 货架编号
        /// </summary>
        public string PodCode { get; set; }

        /// <summary>
        /// 物料批次
        /// </summary>
        public string MaterialLot { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public AgvTaskStatus? AgvTaskStatus { get; set; }

        /// <summary>
        /// 料箱编码
        /// </summary>
        public string BoxCode { get; set; }

        /// <summary>
        /// 料箱容器类型
        /// </summary>
        public string CtnrTyp { get; set; }

        /// <summary>
        /// 起点位置
        /// </summary>
        public string StartPositionCode { get; set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public string EndPositionCode { get; set; }

        /// <summary>
        /// 出库单号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 出库详情
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// AGV编号
        /// </summary>
        public string AgvCode { get; set; }

        /// <summary>
        /// 任务单号
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// 创建时间-开始
        /// </summary>
        public DateTime? CreationTimeStart { get; set; }

        /// <summary>
        /// 创建时间-结束
        /// </summary>
        public DateTime? CreationTimeEnd { get; set; }

        /// <summary>
        /// 请求时间-开始
        /// </summary>
        public DateTime? ReqTimeStart { get; set; }

        /// <summary>
        /// 请求时间-结束
        /// </summary>
        public DateTime? ReqTimeEnd { get; set; }
    }
}
