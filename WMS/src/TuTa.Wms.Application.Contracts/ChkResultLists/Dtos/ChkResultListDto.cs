using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.ChkResultLists.Dtos
{
    public class ChkResultListDto : EntityDto<Guid>
    {
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
        /// 检测单号
        /// </summary>
        public string CheckOrderCode { get; set; }

        /// <summary>
        /// 检验日期
        /// </summary>
        public string CheckDate { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        public string CheckNo { get; set; }

        /// <summary>
        /// 超期复检前的检验单号
        /// </summary>
        public string CheckNoBeforeReCheck { get; set; }

        /// <summary>
        /// 检验类型 
        /// 1(进料检验） 
        /// 2(半成品质检)  
        /// 3(无需检物料收料：第二期放在收料中间表中） 
        /// 4(超期复检)   
        /// 10(期初库存  期初ERP库存生成条码，当检验合格处理）
        /// </summary>
        public string CheckType { get; set; }

        /// <summary>
        /// 检验结论
        /// 1（合格入仓）  
        /// 2（不合格：第一期不合格不进入中间表）  
        /// 3（超筛代用：允许入仓，但需要车间特别注意）
        /// </summary>
        public string CheckResult { get; set; }

        /// <summary>
        /// 检验合格放行数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? PassCnt { get; set; }

        /// <summary>
        /// 供应商编号
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }

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

        public decimal CheckOutCount { get; set; }
    }
}
