using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class BindPodAndBerthDto
    {
        public BindPodAndBerthDto()
        {
        }
        public BindPodAndBerthDto(string reqCode, string positionCode, string podCode, string indBind, string podDir)
        {
            if (reqCode == null)
            {
                ReqCode = Guid.NewGuid().ToString("N");
            }
            else
            {
                ReqCode = reqCode;
            }
            PositionCode = positionCode;
            PodCode = podCode;
            IndBind = indBind;
            PodDir = podDir;
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
        ///  仓位编码
        /// </summary>
        public string PositionCode { get; set; }
        /// <summary>
        /// 料箱编码，解绑时可以为空
        /// </summary>
        public string PodCode { get; set; }

        public string PodDir { get; set; }
        /// <summary>
        /// "1"：绑定， "0"：解绑
        /// </summary>
        public string IndBind { get; set; }

    }
}
