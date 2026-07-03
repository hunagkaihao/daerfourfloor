using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Stocks.Dtos
{
    public class StockDto : EntityDto<Guid>
    {
        /// <summary>
        /// 收料条形码，一次收料生成唯一性条码，WMS作为物料识别码，但可以分成多份与不同的容器进行绑定
        /// </summary>
        public virtual string Barcode { get; set; }

        /// <summary>
        /// 收料条形码（完整码）
        /// </summary>
        public virtual string ReceivingMaterialBarcode { get; set; }

        /// <summary>
        /// 料车ID
        /// </summary>
        //public virtual Guid? VehicleId { get; set; }


        /// <summary>
        /// 所在容器信息
        /// </summary>
        public Guid? BoxId { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        public string BoxCode { get; set; }

        /// <summary>
        /// 容器名称
        /// </summary>
        public string BoxName { get; set; }

        /// <summary>
        /// 所在库位信息
        /// </summary>
        public Guid? CellId { get; set; }

        /// <summary>
        /// 库位编号
        /// </summary>
        public string CellCode { get; set; }

        /// <summary>
        /// 库位名称
        /// </summary>
        public string CellName { get; set; }

        /// <summary>
        /// 所在仓库信息
        /// </summary>
        public virtual Guid? HouseId { get; set; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        public virtual string HouseCode { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public virtual string HouseName { get; set; }

        /// <summary>
        /// 库区Id
        /// </summary>
        public int? AreaId { get; set; }

        /// <summary>
        /// 库区编号
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// 库区名称
        /// </summary>
        public string AreaName { get; set; }



        /// <summary>
        /// 实时物料总数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal TotalCountInTime { get; set; }

        /// <summary>
        /// 库存状态，包括：可用的，冻结的
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 运行状态，包括：待入(In)、库存(Enable)、出库(Out)
        /// </summary>
        public string RunStatus { get; set; }

        /// <summary>
        /// 入库类型  1(正常采购） 2（生产入库：指半成品） 4(委托加工） 7(超期复检）
        /// </summary>
        public string StockInType { get; set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        public string BatchCode { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// 箱号
        /// </summary>
        public string BoxNumber { get; set; }

        /// <summary>
        /// 箱号，对应数据库字段 BoxData_BoxNumber
        /// </summary>
        public string BoxData_BoxNumber { get; set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        public string BLCode { get; set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        public string BHCode { get; set; }

        /// <summary>
        /// 入库日期
        /// </summary>
        public string StockInDate { get; set; }


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
        [Required]
        public virtual string Unit { get; set; }


        /// <summary>
        /// 收料时的总数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal ReceiveTotalCount { get; set; }

        /// <summary>
        /// 收料时的包或箱数
        /// </summary>
        public virtual int? ReceivePkgOrBoxCount { get; set; }

        /// <summary>
        /// 最小包装中的物料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? CountInOnePkgOrBox { get; set; }


        /// <summary>
        /// 检验单号
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

        public decimal? CheckCount { get; set; }

        public decimal? FullBoxRate { get; set; }

        public string AvaType { get; set; }
    }
}
