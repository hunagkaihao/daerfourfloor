using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP出库单DTO
    /// </summary>
    public class ErpOutboundOrderDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 出库单号
        /// </summary>
        public string OutboundOrderNo { get; set; }

        /// <summary>
        /// 仓库代号
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// 计划出库日期
        /// </summary>
        public DateTime PlanOutboundDate { get; set; }

        /// <summary>
        /// 实际出库日期
        /// </summary>
        public DateTime? OutboundDate { get; set; }

        /// <summary>
        /// 出库原因
        /// </summary>
        public string OutboundReason { get; set; }

        /// <summary>
        /// 来源单据
        /// </summary>
        public string SourceDocument { get; set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        public string SourceDocumentNo { get; set; }

        /// <summary>
        /// 经手人
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 出库单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 出库单状态名称
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// 出库单项集合
        /// </summary>
        public List<ErpOutboundItemDto> OutboundItems { get; set; } = new List<ErpOutboundItemDto>();
    }

    /// <summary>
    /// 条码创建出库单请求DTO
    /// </summary>
    public class CreateFromBarcodeDto
    {
        /// <summary>仓库</summary>
        public string WarehouseCode { get; set; }

        /// <summary>客户编码</summary>
        public string CustomerCode { get; set; }

        /// <summary>主表id</summary>
        public string MasterId { get; set; }

        /// <summary>数量</summary>
        public decimal Quantity { get; set; }

        /// <summary>存货编码</summary>
        public string MaterialCode { get; set; }

        /// <summary>包装</summary>
        public string Packaging { get; set; }

        /// <summary>等级</summary>
        public string Grade { get; set; }

        /// <summary>标贴打字</summary>
        public string LabelPrint { get; set; }

        /// <summary>发货单号</summary>
        public string DeliveryOrderNo { get; set; }

        /// <summary>每箱数量</summary>
        public decimal QtyPerBox { get; set; }

        /// <summary>可选U8发货来源明细ID；缺少时本地业务照常，销售推送等待补充。</summary>
        public long? SourceDetailId { get; set; }
        /// <summary>可选销售部门编码。</summary>
        public string SalesDepartmentCode { get; set; }
    }

    /// <summary>
    /// 条码出库记录DTO
    /// </summary>
    public class ErpOutboundRecordDto
    {
        public string Id { get; set; }
        public string Warehouse { get; set; }
        public string CustomerCode { get; set; }
        public string MasterId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QtyPerBox { get; set; }
        public string MaterialCode { get; set; }
        public string Package { get; set; }
        public string Grade { get; set; }
        public string LabelText { get; set; }
        public string DeliveryOrderNo { get; set; }
        public decimal? ActualOutboundQuantity { get; set; }
        public DateTime CreationTime { get; set; }

        /// <summary>U8发货来源明细ID。</summary>
        public long? SourceDetailId { get; set; }
        /// <summary>U8销售部门编码。</summary>
        public string SalesDepartmentCode { get; set; }
        /// <summary>销售单推送结果。</summary>
        public bool IsSalesPushed { get; set; }
        /// <summary>销售单失败原因。</summary>
        public string SalesPushError { get; set; }
        /// <summary>仓管员已确认整单扫码完成。</summary>
        public bool IsDeliveryScanCompleted { get; set; }
        /// <summary>U8销售推送成功时间。</summary>
        public DateTime? SalesPushTime { get; set; }
    }

    /// <summary>
    /// ERP出库单项DTO
    /// </summary>
    public class ErpOutboundItemDto : EntityDto<int>
    {
        /// <summary>
        /// 出库单ID
        /// </summary>
        public Guid OutboundOrderId { get; set; }

        /// <summary>
        /// 材料代号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 计划出库数量
        /// </summary>
        public decimal PlanOutboundQty { get; set; }

        /// <summary>
        /// 实际出库数量
        /// </summary>
        public decimal ActualOutboundQty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// 制令号
        /// </summary>
        public string MoNo { get; set; }

        /// <summary>
        /// 等级代号
        /// </summary>
        public string LevelCode { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string LotNo { get; set; }

        /// <summary>
        /// 储位代号
        /// </summary>
        public string PlaceCode { get; set; }

        /// <summary>
        /// 配送位置代号
        /// </summary>
        public string DeliveryCode { get; set; }

        /// <summary>
        /// 出库项状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 出库项状态名称
        /// </summary>
        public string StatusName { get; set; }
    }
}
