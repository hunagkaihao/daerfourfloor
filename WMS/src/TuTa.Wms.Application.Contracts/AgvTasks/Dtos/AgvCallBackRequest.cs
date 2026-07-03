using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.AgvTasks.Dtos
{
    public class AgvCallBackRequest
    {
        /// <summary>
        /// 任务请求编号，唯一
        /// </summary>
        public string ReqCode { get; set; }
        /// <summary>
        /// 请求时间截 格式: “yyyy-MM-dd HH:mm:ss”。
        /// </summary>
        public string ReqTime { get; set; }
        /// <summary>
        /// TCP 协议必传，REST 协议不用传，。
        /// </summary>
        public string InterfaceName { get; set; }
        /// <summary>
        ///  地码 X 坐标(mm)
        /// </summary>
        public double CooX { get; set; }
        /// <summary>
        /// 地码 Y 坐标(mm)
        /// </summary>
        public double CooY { get; set; }
        /// <summary>
        /// 当前位置编号
        /// </summary>
        public string CurrentPositionCode { get; set; }
        /// <summary>
        /// 自定义字段，不超过 2000 个字符
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 地图编号
        /// </summary>
        public string MapCode { get; set; }
        /// <summary>
        /// 地码编号，唯一标识
        /// </summary>
        public string MapDataCode { get; set; }
        /// <summary>
        /// 方法名, 可使用任务类型做为方法名
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 货架编号
        /// </summary>
        public string PodCode { get; set; }
        /// <summary>
        /// “180”,”0”,”90”,”-90” 分别对应地图
        ///的”左”,”右”,”上”,”下”
        /// </summary>
        public string PodDir { get; set; }
        /// <summary>
        /// AGV 编号（同 agvCode ）
        /// </summary>
        public string RobotCode { get; set; }
        /// <summary>
        /// 当前任务单号
        /// </summary>
        public string TaskCode { get; set; }
        /// <summary>
        /// 工作位，与 RCS-2000 端配置的位置
        ///名称一致
        /// </summary>
        public string WbCode { get; set; }
        /// <summary>
        /// 呼叫点
        /// </summary>
        public string CallCode { get; set; }
        /// <summary>
        /// 料箱编码
        /// </summary>
        public string CtnrCode { get; set; }
        /// <summary>
        /// 当前仓位
        /// </summary>
        public string StgBinCode { get; set; }

    }
}
