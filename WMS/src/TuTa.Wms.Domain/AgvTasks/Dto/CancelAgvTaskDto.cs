using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class CancelAgvTaskDto
    {
        public CancelAgvTaskDto(string reqCode, string taskCode)
        {
            ReqCode = reqCode;
            ReqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ClientCode = "TTWMS";
            TaskCode = taskCode;
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
        /// 任务单号,选填, 不填系统自动生成，必须为 32 位 UUID
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// AGV 编号，填写表示指定某一编号的 AGV 执行该任务
        /// </summary>
        public string AgvCode { get; set; }
        /// <summary>
        /// 任务单号,选填, 不填系统自动生成，必须为 32 位 UUID
        /// </summary>

    }
}
