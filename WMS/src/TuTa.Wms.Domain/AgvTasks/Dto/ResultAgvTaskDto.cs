using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class ResultAgvTaskDto
    {
        public ResultAgvTaskDto()
        {

        }
        public ResultAgvTaskDto(string code, string message, string reqCode, string data)
        {
            Code = code;
            Message = message;
            ReqCode = reqCode;
            Data = data;
        }
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
        /// <summary>
        /// 自定义返回数据
        /// </summary>
        public string Data { get; set; }

    }
}
