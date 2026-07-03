using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TuTa.Wms.BarcodeLists.Dtos
{
    public class BarcodeDto
    {
        public string Id { get; set; }

        /// <summary>
        /// 收料码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 入库类型  1(正常采购） 2（生产入库：指半成品） 4(委托加工） 7(超期复检）
        /// </summary>
        public string StockInType { get; set; }

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
        /// 物料码
        /// </summary>
        public virtual string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public virtual string MaterialName { get; set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        public virtual string Specs { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public virtual string Unit { get; set; }

        /// <summary>
        /// 总收料数量
        /// </summary>
        public decimal ReceiveTotalCount { get; set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public int? ReceivePkgOrBoxCount { get; set; }

        /// <summary>
        /// 最小包装中的物料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? CountInOnePkgOrBox { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 供应商批号
        /// </summary>
        public string SupplierBatchCode { get; set; }

        /// <summary>
        /// 存储仓库信息
        /// </summary>
        public string TargetWarehouseCode { get; set; }

        /// <summary>
        /// 收料仓名称
        /// </summary>
        public string TargetWarehouseName { get; set; }

        /// <summary>
        /// 入库状态
        /// </summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// 已入库数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal InBoundedCount { get; set; }

        /// <summary>
        /// 已绑定数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal InBindCount { get; set; }



        /// <summary>
        /// 剩余绑定数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal SurplusCount { get; set; }

        /// <summary>
        /// 已抽检数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal InCheckOutCount { get; set; }

        public virtual decimal MaxCount { get; set; }

        /// <summary>
        /// 收料日期
        /// </summary>
        public DateTime SLDate { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PurchaseId { get; set; }

    }
}
