using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class ResultAgvTaskStatusDto
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 请求编号
        /// </summary>
        public string ReqCode { get; set; }
        public bool Interrupt { get; set; }
        /// <summary>
        ///  任务状态列表
        /// </summary>
        public List<ResultDataAgvTaskStatus> Data { get; set; }

    }
    public class ResultDataAgvTaskStatus
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public string TaskTyp { get; set; }
        /// <summary>
        /// 任务状态：0-发送异常，1-已创建
        /// </summary>
        public string TaskStatus { get; set; }

    }
}
