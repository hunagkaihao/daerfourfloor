using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using Volo.Abp;

namespace TuTa.Wms.Stocks.Events
{
    public class StockMoveEvent
    {
        public StockMoveEvent(
            DateTime moveDate,
            string gysCode, string gysName,
            string materialCode, string materialName, string specs, string unit,
            string checkNo,
            string barcode,
            decimal moveCount,
            int moveType,
            string operatorName)
        {
            MoveDate = moveDate;
            SupplierCode = gysCode;
            SupplierName = gysName;
            MaterialCode = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            MaterialName = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            MaterialSpecs = specs;
            MaterialUnit = unit;
            CheckNo = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            MoveCount = Check.Positive(moveCount, nameof(moveCount));
            MoveType = moveType;
            OperatorName = operatorName;
        }

        /// <summary>
        /// 调拨日期
        /// </summary>
        public DateTime MoveDate { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [Required]
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string MaterialSpecs { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string MaterialUnit { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        public string CheckNo { get; set; }

        /// <summary>
        /// 收料码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 调拨数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal MoveCount { get; set; }

        /// <summary>
        /// 调拨类型
        /// 1 暂存调正常  2正常调暂存
        /// </summary>
        public int MoveType { get; set; }

        /// <summary>
        /// 调拨人员
        /// </summary>
        public string OperatorName { get; set; }
    }
}
