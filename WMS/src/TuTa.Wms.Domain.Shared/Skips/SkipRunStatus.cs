using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Skips
{
    public enum SkipRunStatus
    {
        /// <summary>
        /// 未使用
        /// </summary>
        Enable,
        /// <summary>
        /// 入库
        /// </summary>
        In,
        /// <summary>
        /// 车间出库
        /// </summary>
        OutByWork,
        /// <summary>
        /// 仓库出库
        /// </summary>
        OutByWare
    }
    public static class SkipRunStatusHelper
    {
        public static string SkipRunStatusToChinese(SkipRunStatus type)
        {
            switch (type)
            {
                case SkipRunStatus.Enable:
                    return "待使用";
                case SkipRunStatus.In:
                    return "入库";
                case SkipRunStatus.OutByWork:
                    return "车间出库";
                case SkipRunStatus.OutByWare:
                    return "仓库出库";
                default: throw new Exception($"无效的类型：{type.ToString()}");
            }
        }

        public static SkipRunStatus ChineseToStockInType(string chinese)
        {
            switch (chinese)
            {
                case "待使用":
                    return SkipRunStatus.Enable;
                case "入库":
                    return SkipRunStatus.In;
                case "车间出库":
                    return SkipRunStatus.OutByWork;
                case "仓库出库":
                    return SkipRunStatus.OutByWare;
                default: throw new Exception($"无效的类型：{chinese}");
            }
        }

        public static SkipRunStatus SkipRunStatusCheck(int value, string parameterName)
        {
            if (!Enum.IsDefined(typeof(SkipRunStatus), value))
                throw new Exception($"{parameterName}的值{value}无效");

            return (SkipRunStatus)value;
        }
    }
}
