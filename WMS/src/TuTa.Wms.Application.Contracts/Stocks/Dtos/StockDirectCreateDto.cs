using TuTa.Wms.Stocks;

namespace TuTa.Wms.Stocks.Dtos
{
    /// <summary>
    /// 直接创建库存的 DTO
    /// </summary>
    public class StockDirectCreateDto
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specs { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 库存总数
        /// </summary>
        public decimal TotalCount { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 供应商批次号
        /// </summary>
        public string SupplierBatchCode { get; set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        public string BatchCode { get; set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        public string BLCode { get; set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        public string BHCode { get; set; }

        /// <summary>
        /// 入库类型
        /// </summary>
        public StockInType StockInType { get; set; } = StockInType.PurchaseStockIn; // 默认为正常采购
    }
}
