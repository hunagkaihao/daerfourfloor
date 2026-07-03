using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.Stocks;

using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.AgvTasks.Aggregaes
{
    public class AgvTask : FullAuditedAggregateRoot<int>
    {
        /// <summary>
        /// 海康AGV调度任务接口
        /// </summary>
        private AgvTask()
        {
            //ManageStatus = ManageStatus.WaitingExecute;
            //PositionCodePath = new List<AgvPosition>();
        }
        public AgvTask(string reqCode, string clientCode, string taskTyp, string wbCode, string podCode, string materialLot)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = clientCode;
            TaskTyp = taskTyp;
            WbCode = wbCode;
            PodCode = podCode;
            MaterialLot = materialLot;
            //SetAsCompleated("Completed");
            AgvTaskStatus = AgvTaskStatus.Created;
        }
        public AgvTask(string reqCode, string taskTyp, string podCode
, string[] userCallCodePath, string boxCode, string startPositionCode
, string endPositionCode, string ctnrTyp, ManageType type)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            StockTyp = type;
            PodCode = podCode;
            UserCallCodePath = JsonConvert.SerializeObject(userCallCodePath);
            //SetAsCompleated("Completed");
            //PositionCodePath = new List<AgvPosition>();
            AgvTaskStatus = AgvTaskStatus.Created;
            BoxCode = boxCode;
            StartPositionCode = startPositionCode;
            EndPositionCode = endPositionCode;
            CtnrTyp = ctnrTyp;
        }

        public AgvTask(string reqCode, string taskTyp, string podCode
, string[] userCallCodePath, string boxCode, string startPositionCode
, string endPositionCode, string ctnrTyp, ManageType type, string picklist, string unique)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            StockTyp = type;
            PodCode = podCode;
            UserCallCodePath = JsonConvert.SerializeObject(userCallCodePath);
            //SetAsCompleated("Completed");
            //PositionCodePath = new List<AgvPosition>();
            AgvTaskStatus = AgvTaskStatus.Created;
            BoxCode = boxCode;
            StartPositionCode = startPositionCode;
            EndPositionCode = endPositionCode;
            CtnrTyp = ctnrTyp;
            PickListCode = picklist;
            UniqueCode = unique;
        }

        public AgvTask(string reqCode, string taskTyp, string podCode
, string[] userCallCodePath, string startPositionCode
, string endPositionCode, ManageType type)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            StockTyp = type;
            PodCode = podCode;
            UserCallCodePath = JsonConvert.SerializeObject(userCallCodePath);
            //SetAsCompleated("Completed");
            //PositionCodePath = new List<AgvPosition>();
            AgvTaskStatus = AgvTaskStatus.Created;
            StartPositionCode = startPositionCode;
            EndPositionCode = endPositionCode;
        }
        public void Update(string reqCode, string clientCode, string taskTyp, string wbCode, string podCode, string materialLot)
        {
            ReqCode = reqCode;
            ClientCode = clientCode;
            TaskTyp = taskTyp;
            WbCode = wbCode;
            PodCode = podCode;
            MaterialLot = materialLot;
        }

        /// <summary>
        /// 任务请求编号，唯一
        /// </summary>
        public string ReqCode { get; set; }
        /// <summary>
        /// 请求时间截 格式: “yyyy-MM-dd HH:mm:ss”。
        /// </summary>
        public string ReqTime { get; set; }
        /// <summary>
        /// 客户端编号，如PDA，HCWMS等。 由RCS-2000告知上层系统
        /// </summary>
        public string ClientCode { get; set; }
        /// <summary>
        /// 令 牌 号 , 由 调 度 系 统 颁 发 。 由RCS-2000 告知上层系统
        /// </summary>
        public string TokenCode { get; set; }
        /// <summary>
        /// 任务类型，与在 RCS-2000 端配置的主任务类型编号一致。
        /// </summary>
        public string TaskTyp { get; set; }

        /// <summary>
        /// 物料任务类型
        /// </summary>
        public ManageType StockTyp { get; set; }
        /// <summary>
        /// 工作位，一般为机台或工作台位置，
        /// 与 RCS-2000 端配置的位置名称一
        /// 致, 位置名称为字母\数字\或组合, 不超过 32 位。        /// 
        /// </summary>
        public string WbCode { get; set; }
        /// <summary>
        /// 位置路径：AGV 关键路径位置集合，
        /// 与任务类型中模板配置的位置路径一一对应。
        /// 待现场地图部署、配置完成后可获取
        /// </summary>
        //public List<AgvPosition> PositionCodePath { get; set; }
        /// <summary>
        /// 货架编号，不指定货架可以为空
        /// </summary>
        public string PodCode { get; set; }
        /// <summary>
        /// “180”,”0”,”90”,”-90” 分别对应地图
        /// 的”左”,”右”,”上”,”下” ，不指定方向可以为空
        /// </summary>
        public string PodDir { get; set; }
        /// <summary>
        /// 货架类型, 找满货架时传空, 找空货架时必传
        /// -1: 代表不关心货架类型, 找到空货架即可.
        /// -2: 代表从工作位获取关联货架类型, 如果未配置, 只找空货架.
        /// 货架类型编号: 只找该货架类型的空货架
        /// </summary>
        public string PodTyp { get; set; }
        /// <summary>
        /// 物料批次或货架上的物料唯一编码,生成任务单时,货架与物料直接绑定时使用. 
        /// （通过同时传 podCode 和materialLot 来 绑 定 或 通 过positionCode 找到位置上的货架和materialLot 来绑定）
        /// </summary>
        public string MaterialLot { get; set; }
        /// <summary>
        /// 优先级，从（1~5）级，最大优先级最高
        /// </summary>
        public string Priority { get; set; }
        /// <summary>
        /// 任务单号,选填, 不填系统自动生成，必须为 32 位 UUID
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
        /// 任务类型
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
        /// 任务开始时间
        /// </summary>
        public DateTime? TaskStartTime { get; set; }

        public void SetAsCreated()
        {
            AgvTaskStatus = AgvTaskStatus.Created;
        }

        public void SetAsWaitingExecuting()
        {
            AgvTaskStatus = AgvTaskStatus.WaitingExecuting;
        }

        public void SetAsExecuting()
        {
            AgvTaskStatus = AgvTaskStatus.Executing;
        }
        /// <summary>
        /// 设置AGV任务开始
        /// </summary>
        public void SetAsTaskStart()
        {
            AgvTaskStatus = AgvTaskStatus.TaskStart;

        }
        public void SetAsCellOut()
        {
            AgvTaskStatus = AgvTaskStatus.CellOut;
            TaskStartTime = DateTime.Now;
        }
        public void SetAsCancel()
        {
            AgvTaskStatus = AgvTaskStatus.Cancel;
        }
        public void SetAsCompleted()
        {
            AgvTaskStatus = AgvTaskStatus.Complete;
        }
    }
}
