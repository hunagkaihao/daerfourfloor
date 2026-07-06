using System;

namespace TuTa.Wms.Stocks.Dtos
{
    public class StockCreateDto
    {
        /// <summary>
        /// 收料条形码（兼容旧传参；容器组盘建议使用 MaterialCode）
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 物料码（容器组盘建议传该字段）
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 入库数量（库存数量）
        /// </summary>
        public decimal TotalCount { get; set; }
        /// <summary>
        /// 入库包或箱数
        /// </summary>
        public int TotalPagOrBox { get; set; }

        /// <summary>
        /// 箱号（可选，用于标识物料所在箱子）
        /// </summary>
        public string BoxNumber { get; set; }

        /// <summary>
        /// 生产批次
        /// </summary>
        public string BatchCode { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        public string ProcessNo { get; set; }

        /// <summary>
        /// 供应商生产批次
        /// </summary>
        public string SupplierBatchCode { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? SupplierProductionDate { get; set; }

        /// <summary>
        /// 收料条形码
        /// </summary>
        public string ReceivingMaterialBarcode { get; set; }

        /// <summary>
        /// ASN码（入库单号）
        /// </summary>
        public string AsnCode { get; set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public int? ReceivePkgOrBoxCount { get; set; }

        /// <summary>
        /// 最小包装中的物料数量（每箱数量）
        /// </summary>
        public decimal? CountInOnePkgOrBox { get; set; }
    }
}
