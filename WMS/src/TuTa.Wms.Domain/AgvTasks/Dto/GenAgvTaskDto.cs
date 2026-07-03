using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class GenAgvTaskDto
    {
        /// <summary>
        /// 指定货架任务
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="taskTyp"></param>
        /// <param name="userCallCodePath"></param>
        /// <param name="taskCode"></param>
        /// <param name="podCode"></param>
        public GenAgvTaskDto(string reqCode, string taskTyp, string[] userCallCodePath, string taskCode, string podCode)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            UserCallCodePath = userCallCodePath;
            TaskCode = taskCode;
            PodCode = podCode;
        }

        /// <summary>
        /// 创建CTU任务
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="taskTyp"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="taskCode"></param>
        /// <param name="userCallCodePath"></param>
        public GenAgvTaskDto(string reqCode, string taskTyp, string ctnrTyp, string taskCode, string[] userCallCodePath, string ctnrCode)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            UserCallCodePath = userCallCodePath;
            TaskCode = taskCode;
            CtnrTyp = ctnrTyp;//容器类型
            CtnrCode = ctnrCode;//料箱编码
        }

        /// <summary>
        /// 创建入库任务
        /// </summary>
        /// <param name="reqCode"></param>
        /// <param name="taskTyp"></param>
        /// <param name="ctnrTyp"></param>
        /// <param name="taskCode"></param>
        /// <param name="userCallCodePath"></param>
        public GenAgvTaskDto(string reqCode, string taskTyp, string ctnrTyp, string taskCode, string[] userCallCodePath, string ctnrCode,string podCode)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskTyp = taskTyp;
            UserCallCodePath = userCallCodePath;
            TaskCode = taskCode;
            CtnrTyp = ctnrTyp;//容器类型
            CtnrCode = ctnrCode;//料箱编码
            PodCode = podCode;
        }
        public GenAgvTaskDto(string reqCode)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
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
        /// 
        /// </summary>
        public string TaskTyp { get; set; }
        /// <summary>
        /// 工作位，一般为机台或工作台位置，
        /// 与 RCS-2000 端配置的位置名称一
        /// 致, 位置名称为字母\数字\或组合, 不超过 32 位。        /// 
        /// </summary>
        public string WbCode { get; set; }
        /// <summary>
        /// 站点集合
        /// </summary>
        public string[] UserCallCodePath { get; set; }
        /// <summary>
        /// 任务单号
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// 货架编号，不指定货架可以为空
        /// </summary>
        public string PodCode { get; set; }
        /// <summary>
        /// 货架类型, 找满货架时传空, 找空
        ///货架时必传
        /// </summary>
        public string PodTyp { get; set; }
        /// <summary>
        /// AGV 编号，填写表示指定某一编号
        ///的 AGV 执行该任务
        /// </summary>
        public string AgvCode { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public string Priority { get; set; }
        /// <summary>
        /// CTU料箱类型
        /// </summary>
        public string CtnrTyp { get; set; }
        /// <summary>
        /// CTU料箱编码
        /// </summary>
        public string CtnrCode { get; set; }


    }
}
