using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP入库单DTO
    /// </summary>
    public class ErpInboundOrderDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 入库单号
        /// </summary>
        [Required]
        public string InboundOrderNo { get; set; }

        /// <summary>
        /// 仓库代号
        /// </summary>
        [Required]
        public string WarehouseCode { get; set; }

        /// <summary>
        /// 计划入库日期
        /// </summary>
        [Required]
        public DateTime PlanInboundDate { get; set; }

        /// <summary>
        /// 实际入库日期
        /// </summary>
        public DateTime? ActualInboundDate { get; set; }

        /// <summary>
        /// 入库原因
        /// </summary>
        public string InboundReason { get; set; }

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
        /// 入库单状态
        /// </summary>
        [Required]
        public int Status { get; set; }

        /// <summary>
        /// 入库单项列表
        /// </summary>
        public List<ErpInboundItemDto> InboundItems { get; set; } = new List<ErpInboundItemDto>();
    }

    /// <summary>
    /// ERP入库单项DTO
    /// </summary>
    public class ErpInboundItemDto : EntityDto<int>
    {
        /// <summary>
        /// 入库单ID
        /// </summary>
        [Required]
        public Guid InboundOrderId { get; set; }

        /// <summary>
        /// 材料代号
        /// </summary>
        [Required]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [Required]
        public string MaterialName { get; set; }

        /// <summary>
        /// 计划入库数量
        /// </summary>
        [Required]
        public decimal PlanInboundQty { get; set; }

        /// <summary>
        /// 实际入库数量
        /// </summary>
        [Required]
        public decimal ActualInboundQty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [Required]
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
        /// 规格型号
        /// </summary>
        public string Specs { get; set; }

        /// <summary>
        /// 入库项状态
        /// </summary>
        [Required]
        public int Status { get; set; }
    }
}
