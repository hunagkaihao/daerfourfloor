using System;

namespace TuTa.Wms.ChkResultLists
{
    public enum EnumCheckType
    {
        /// <summary>
        /// 进料检验
        /// </summary>
        StockInCheck = 1,

        /// <summary>
        /// 半成品检验
        /// </summary>
        SemiProductCheck = 2,

        /// <summary>
        /// 无需检物料收料：第二期放在收料中间表中
        /// </summary>
        NoCheck = 3,

        /// <summary>
        /// 超期复检
        /// </summary>
        ReCheck = 4,

        /// <summary>
        /// 期初库存（期初ERP库存生成条码，当检验合格处理）
        /// </summary>
        InitialStock = 10,

        /// <summary>
        /// 车间退货检验
        /// </summary>
        GoodsReturnCheck = 18
    }

    public static class CheckTypeHelper
    {
        public static string CheckTypeToChinese(EnumCheckType checkType)
        {
            switch (checkType)
            {
                case EnumCheckType.StockInCheck:
                    return "进料检验";
                case EnumCheckType.SemiProductCheck:
                    return "半成品质检";
                case EnumCheckType.NoCheck:
                    return "无需检验收料";
                case EnumCheckType.ReCheck:
                    return "超期复检";
                case EnumCheckType.InitialStock:
                    return "初期库存";
                case EnumCheckType.GoodsReturnCheck:
                    return "车间退货检验";
                default:
                    throw new Exception($"未知检验类型: {checkType.ToString()}");
            }
        }

        public static EnumCheckType ChineseToCheckType(string chinese)
        {
            switch (chinese)
            {
                case "进料检验":
                    return EnumCheckType.StockInCheck;
                case "半成品质检":
                    return EnumCheckType.SemiProductCheck;
                case "无需检验收料":
                    return EnumCheckType.NoCheck;
                case "超期复检":
                    return EnumCheckType.ReCheck;
                case "初期库存":
                    return EnumCheckType.InitialStock;
                case "车间退货检验":
                    return EnumCheckType.GoodsReturnCheck;
                default:
                    throw new Exception($"未知检验类型: {chinese}");
            }
        }

        public static EnumCheckType CheckTypeCheck(int value, string parameterName)
        {
            if (!Enum.IsDefined(typeof(EnumCheckType), value))
                throw new Exception($"{parameterName}的值{value}无效");

            return (EnumCheckType)value;
        }
    }
}
