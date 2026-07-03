using System;
using TuTa.Wms.AgvTasks;
using TuTa.Wms.Stocks;

namespace TuTa.Wms.AgvTasks.Dtos
{
    /// <summary>
    /// AGV任务数据传输对象
    /// </summary>
    public class AgvTaskDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务请求编号，唯一
        /// </summary>
        public string ReqCode { get; set; }

        /// <summary>
        /// 请求时间截 格式: "yyyy-MM-dd HH:mm:ss"
        /// </summary>
        public string ReqTime { get; set; }

        /// <summary>
        /// 客户端编号，如PDA，HCWMS等
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// 令牌号，由调度系统颁发
        /// </summary>
        public string TokenCode { get; set; }

        /// <summary>
        /// 任务类型，与在 RCS-2000 端配置的主任务类型编号一致
        /// </summary>
        public string TaskTyp { get; set; }

        /// <summary>
        /// 物料任务类型
        /// </summary>
        public ManageType? StockTyp { get; set; }

        /// <summary>
        /// 工作位，一般为机台或工作台位置
        /// </summary>
        public string WbCode { get; set; }

        /// <summary>
        /// 货架编号，不指定货架可以为空
        /// </summary>
        public string PodCode { get; set; }

        /// <summary>
        /// 货架方向
        /// </summary>
        public string PodDir { get; set; }

        /// <summary>
        /// 货架类型
        /// </summary>
        public string PodTyp { get; set; }

        /// <summary>
        /// 物料批次或货架上的物料唯一编码
        /// </summary>
        public string MaterialLot { get; set; }

        /// <summary>
        /// 优先级，从（1~5）级，最大优先级最高
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// 任务单号，选填，不填系统自动生成，必须为 32 位 UUID
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// AGV 编号，填写表示指定某一编号的 AGV 执行该任务
        /// </summary>
        public string AgvCode { get; set; }

        /// <summary>
        /// 自定义字段，不超过 2000 个字符
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 站点集合
        /// </summary>
        public string UserCallCodePath { get; set; }

        /// <summary>
        /// 关联任务
        /// </summary>
        public string RefTask { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public AgvTaskStatus AgvTaskStatus { get; set; }

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
        /// 出库单
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 出库详情
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// 最后修改者ID
        /// </summary>
        public Guid? LastModifierId { get; set; }

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public DateTime? TaskStartTime { get; set; }
    }
}
