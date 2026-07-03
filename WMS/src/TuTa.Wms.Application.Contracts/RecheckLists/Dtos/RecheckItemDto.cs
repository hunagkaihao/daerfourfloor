using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuTa.Wms.RecheckLists.Dtos
{
    public class RecheckItemDto
    {
        /// <summary>
        /// 复检单单号
        /// </summary>
        public string RecheckListCode { get; set; }

        /// <summary>
        /// 复检单日期
        /// </summary>
        public string RecheckListDate { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        public string CheckNo { get; set; }

        /// <summary>
        /// 收料码
        /// </summary>
        public string Barcode { get; set; }
        
        /// <summary>
        /// 物料码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string MaterialSpecs { get; set; }

        /// <summary>
        /// 物料单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 质保期天数
        /// </summary>
        public int? ExpiryDays { get; set; }

        /// <summary>
        /// 保质期限
        /// </summary>
        public DateTime? ExpiryLimitDate { get; set; }

        /// <summary>
        /// 复检次数
        /// </summary>
        public int? RecheckTimes { get; set; }

        /// <summary>
        /// 复检数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CheckCount { get; set; }

        /// <summary>
        /// 抽检数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal SampleCount { get; set; }

        /// <summary>
        /// 已领料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickedCount { get; set; }               

        /// <summary>
        /// 复检项状态
        /// </summary>
        public string Status { get; set; }
    }
}
