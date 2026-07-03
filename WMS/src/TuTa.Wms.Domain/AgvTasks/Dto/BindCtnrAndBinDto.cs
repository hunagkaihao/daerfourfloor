using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Dto
{
    public class BindCtnrAndBinDto
    {
        public BindCtnrAndBinDto()
        {
        }
        public BindCtnrAndBinDto(string reqCode, string stgBinCode, string ctnrTyp, string ctnrCode, string indBind)
        {
            if (reqCode == null)
            {
                ReqCode = Guid.NewGuid().ToString("N");
            }
            else
            {
                ReqCode = reqCode;
            }
            StgBinCode = stgBinCode;
            CtnrTyp = ctnrTyp;
            CtnrCode = ctnrCode;
            CtnrNum = "1";//默认给1
            IndBind = indBind;
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
        public string StgBinCode { get; set; }
        /// <summary>
        /// 料箱编码，解绑时可以为空
        /// </summary>
        public string CtnrCode { get; set; }

        /// <summary>
        /// 仓位编码  计量院中2代表大料箱，1代表小料箱
        /// </summary>
        public string CtnrTyp { get; set; }

        /// <summary>
        /// 容器数量,默认1
        /// </summary>
        public string CtnrNum { get; set; }
        /// <summary>
        /// "1"：绑定， "0"：解绑
        /// </summary>
        public string IndBind { get; set; }

    }
}
