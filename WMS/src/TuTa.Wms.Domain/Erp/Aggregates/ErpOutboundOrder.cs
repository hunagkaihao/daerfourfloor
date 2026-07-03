using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TuTa.Wms.Erp.Entities;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP出库单聚合根
    /// </summary>
    public class ErpOutboundOrder : AuditedAggregateRoot<Guid>
    {
        private ErpOutboundOrder()
        {
        }

        internal ErpOutboundOrder(
            Guid id,
            string outboundOrderNo,
            string warehouseCode,
            DateTime planOutboundDate,
            string outboundReason = null,
            string sourceDocument = null,
            string sourceDocumentNo = null,
            string handler = null,
            string remarks = null)
        {
            Check.NotNullOrWhiteSpace(outboundOrderNo, nameof(outboundOrderNo));
            Check.NotNullOrWhiteSpace(warehouseCode, nameof(warehouseCode));

            Id = id;
            OutboundOrderNo = outboundOrderNo;
            WarehouseCode = warehouseCode;
            PlanOutboundDate = planOutboundDate;
            OutboundReason = outboundReason;
            SourceDocument = sourceDocument;
            SourceDocumentNo = sourceDocumentNo;
            Handler = handler;
            Remarks = remarks;
            Status = OutboundOrderStatus.Created;
            ActualOutboundDate = null;
            OutboundItems = new List<ErpOutboundItem>();
        }

        /// <summary>
        /// 添加出库单项
        /// </summary>
        public void AddOutboundItem(
            string materialCode,
            string materialName,
            decimal planOutboundQty,
            decimal actualOutboundQty,
            string unitCode,
            string moNo = null,
            string levelCode = null,
            string lotNo = null,
            string placeCode = null,
            string deliveryCode = null)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.Positive(planOutboundQty, nameof(planOutboundQty));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));

            // 检查是否已存在相同物料编码的出库项
            //var existingItem = OutboundItems.FirstOrDefault(o => o.MaterialCode == materialCode);
            //if (existingItem != null)
            //    throw new Exception($"出库单中已存在物料编码为{materialCode}的出库项，不能重复添加");

            var outboundItem = new ErpOutboundItem(
                Id,
                materialCode,
                materialName,
                planOutboundQty,
                actualOutboundQty,
                unitCode,
                moNo,
                levelCode,
                lotNo,
                placeCode,
                deliveryCode);

            OutboundItems.Add(outboundItem);
        }

        /// <summary>
        /// 移除出库单项
        /// </summary>
        public void RemoveOutboundItem(string materialCode)
        {
            var item = OutboundItems.FirstOrDefault(o => o.MaterialCode == materialCode);
            if (item == null)
                return;

            OutboundItems.Remove(item);
        }

        /// <summary>
        /// 设置出库单状态
        /// </summary>
        public void SetStatus(OutboundOrderStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// 设置实际出库日期
        /// </summary>
        internal void SetActualOutboundDate(DateTime actualOutboundDate)
        {
            ActualOutboundDate = actualOutboundDate;
        }

        /// <summary>
        /// 出库单号
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string OutboundOrderNo { get; private set; }

        /// <summary>
        /// 仓库代号
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string WarehouseCode { get; private set; }

        /// <summary>
        /// 计划出库日期
        /// </summary>
        [Required]
        public virtual DateTime PlanOutboundDate { get; private set; }

        /// <summary>
        /// 实际出库日期
        /// </summary>
        public virtual DateTime? ActualOutboundDate { get; private set; }

        /// <summary>
        /// 出库原因
        /// </summary>
        [StringLength(100)]
        public virtual string OutboundReason { get; private set; }

        /// <summary>
        /// 来源单据
        /// </summary>
        [StringLength(50)]
        public virtual string SourceDocument { get; private set; }

        /// <summary>
        /// 来源单号
        /// </summary>
        [StringLength(50)]
        public virtual string SourceDocumentNo { get; private set; }

        /// <summary>
        /// 经手人
        /// </summary>
        [StringLength(20)]
        public virtual string Handler { get; private set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public virtual string Remarks { get; private set; }

        /// <summary>
        /// 出库单状态
        /// </summary>
        [Required]
        public virtual OutboundOrderStatus Status { get; private set; }

        /// <summary>
        /// 出库单项集合
        /// </summary>
        public virtual List<ErpOutboundItem> OutboundItems { get; private set; }
    }

    /// <summary>
    /// 出库单状态枚举
    /// </summary>
    public enum OutboundOrderStatus
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created = 1,

        /// <summary>
        /// 出库中
        /// </summary>
        Outbounding = 2,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 3,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 4
    }
}
