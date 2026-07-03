using System;

namespace TuTa.Wms.Stocks
{
    public enum StockInType
    {
        /// <summary>
        /// 采购入库
        /// </summary>
        PurchaseStockIn = 1,

        /// <summary>
        /// 生产入库
        /// </summary>
        ProductionStockIn = 2,

        /// <summary>
        /// 委托加工入库
        /// </summary>
        DelegateStockIn = 4,

        /// <summary>
        /// 盘点入库
        /// </summary>
        InventoryStockIn= 5,

        /// <summary>
        /// 超期复检入库
        /// </summary>
        RecheckStockIn = 7,

        /// <summary>
        /// 期初入库
        /// </summary>
        OpeningStockIn = 10,

        /// <summary>
        /// 车间退货
        /// </summary>
        GoodsReturn = 18,

        /// <summary>
        /// 调整库存入库，用于不正确出库时，重新入库时用
        /// </summary>
        AdjustStockIn = 20
    }

    public static class StockInTypeHelper
    {
        public static string StockInTypeToChinese(StockInType type)
        {
            switch (type)
            {
                case StockInType.PurchaseStockIn: 
                    return "正常采购";
                case StockInType.ProductionStockIn: 
                    return "生产入库";
                case StockInType.DelegateStockIn: 
                    return "委托加工";
                case StockInType.InventoryStockIn: 
                    return "盘点入库";
                case StockInType.RecheckStockIn:
                    return "超期复检";
                case StockInType.GoodsReturn:
                    return "车间退货";
                case StockInType.AdjustStockIn:
                    return "调整库存入库";
                case StockInType.OpeningStockIn:
                    return "期初入库";
                default: throw new Exception($"无效的入库类型：{type.ToString()}");
            }
        }

        public static StockInType ChineseToStockInType(string chinese)
        {
            switch (chinese)
            {
                case "正常采购":
                    return StockInType.PurchaseStockIn;
                case "生产入库":
                    return StockInType.ProductionStockIn;
                case "委托加工":
                    return StockInType.DelegateStockIn;
                case "盘点入库":
                    return StockInType.InventoryStockIn;
                case "超期复检":
                    return StockInType.RecheckStockIn;
                case "车间退货":
                    return StockInType.GoodsReturn;
                case "调整库存入库":
                    return StockInType.AdjustStockIn;
                case "期初入库":
                    return StockInType.OpeningStockIn;
                default: throw new Exception($"无效的入库类型：{chinese}");
            }
        }

        public static StockInType StockInTypeCheck(int value, string parameterName)
        {
            if (!Enum.IsDefined(typeof(StockInType), value))
                throw new Exception($"{parameterName}的值{value}无效");

            return (StockInType)value;
        }
    }

}
