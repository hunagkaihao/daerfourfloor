using System;

namespace TuTa.Wms.ChkResultLists
{
    public enum EnumCheckResult
    {
        /// <summary>
        /// 合格
        /// </summary>
        Pass = 1,
        /// <summary>
        /// 不合格
        /// </summary>
        NoPass = 2,
        /// <summary>
        /// 筛选待用（允许入仓，但需要车间特别注意）
        /// </summary>
        Filter = 3
    }

    public static class CheckResultHelper
    {
        public static string CheckResultToChinese(EnumCheckResult checkResult)
        {
            switch (checkResult)
            {
                case EnumCheckResult.Pass:
                    return "合格";
                case EnumCheckResult.NoPass:
                    return "不合格";
                case EnumCheckResult.Filter:
                    return "超筛待用";
                default:
                    throw new Exception($"未知检验结果: {checkResult.ToString()}");
            }
        }

        public static EnumCheckResult ChineseToCheckResult(string chinese)
        {
            switch (chinese)
            {
                case "合格":
                    return EnumCheckResult.Pass;
                case "不合格":
                    return EnumCheckResult.NoPass;
                case "超筛待用":
                    return EnumCheckResult.Filter;
                default:
                    throw new Exception($"未知检验结果: {chinese}");
            }
        }

        public static EnumCheckResult CheckResultCheck(int value, string parameterName)
        {
            if (!Enum.IsDefined(typeof(EnumCheckResult), value))
                throw new Exception($"{parameterName}的值{value}无效");

            return (EnumCheckResult)value;
        }
    }
}
