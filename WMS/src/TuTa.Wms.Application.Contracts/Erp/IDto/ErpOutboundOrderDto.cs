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
        public DateTime? ActualOutboundDate { get; set; }

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
