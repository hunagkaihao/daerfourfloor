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
    /// ERP入库单聚合根
    /// </summary>
    public class ErpInboundOrder : AuditedAggregateRoot<Guid>
    {
        private ErpInboundOrder()
        {
        }

        internal ErpInboundOrder(
            Guid id,
            string inboundOrderNo,
            string warehouseCode,
            DateTime planInboundDate,
            string inboundReason = null,
            string sourceDocument = null,
            string sourceDocumentNo = null,
            string handler = null,
            string remarks = null)
        {
            Check.NotNullOrWhiteSpace(inboundOrderNo, nameof(inboundOrderNo));
            Check.NotNullOrWhiteSpace(warehouseCode, nameof(warehouseCode));

            Id = id;
            InboundOrderNo = inboundOrderNo;
            WarehouseCode = warehouseCode;
            PlanInboundDate = planInboundDate;
            InboundReason = inboundReason;
            SourceDocument = sourceDocument;
            SourceDocumentNo = sourceDocumentNo;
            Handler = handler;
            Remarks = remarks;
            Status = InboundOrderStatus.Created;
            ActualInboundDate = null;
            InboundItems = new List<ErpInboundItem>();
        }

        /// <summary>
        /// 添加入库单项
        /// </summary>
        public void AddInboundItem(
            string materialCode,
            string materialName,
            decimal planInboundQty,
            decimal actualInboundQty,
            string unitCode,
            string moNo = null,
            string levelCode = null,
            string lotNo = null,
            string specs = null)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.Positive(planInboundQty, nameof(planInboundQty));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));

            // 检查是否已存在相同物料编码的入库项
            //var existingItem = InboundItems.FirstOrDefault(o => o.MaterialCode == materialCode);
            //if (existingItem != null)
            //    throw new Exception($"入库单中已存在物料编码为{materialCode}的入库项，不能重复添加");

            var inboundItem = new ErpInboundItem(
                Id,
                materialCode,
                materialName,
                planInboundQty,
                actualInboundQty,
                unitCode,
                moNo,
                levelCode,
                lotNo,
                specs);

            InboundItems.Add(inboundItem);
        }

        /// <summary>
        /// 移除入库单项
        /// </summary>
        public void RemoveInboundItem(string materialCode)
        {
            var item = InboundItems.FirstOrDefault(o => o.MaterialCode == materialCode);
            if (item == null)
                return;

            InboundItems.Remove(item);
        }

        /// <summary>
        /// 设置入库单状态
        /// </summary>
        public void SetStatus(InboundOrderStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// 设置实际入库日期
        /// </summary>
        internal void SetActualInboundDate(DateTime actualInboundDate)
        {
            ActualInboundDate = actualInboundDate;
        }

        /// <summary>
        /// 入库单号
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string InboundOrderNo { get; private set; }

        /// <summary>
        /// 仓库代号
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string WarehouseCode { get; private set; }

        /// <summary>
        /// 计划入库日期
        /// </summary>
        [Required]
        public virtual DateTime PlanInboundDate { get; private set; }

        /// <summary>
        /// 实际入库日期
        /// </summary>
        public virtual DateTime? ActualInboundDate { get; private set; }

        /// <summary>
        /// 入库原因
        /// </summary>
        [StringLength(100)]
        public virtual string InboundReason { get; private set; }

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
        /// 入库单状态
        /// </summary>
        [Required]
        public virtual InboundOrderStatus Status { get; private set; }

        /// <summary>
        /// 入库单项集合
        /// </summary>
        public virtual List<ErpInboundItem> InboundItems { get; private set; }
    }

    /// <summary>
    /// 入库单状态枚举
    /// </summary>
    public enum InboundOrderStatus
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created = 1,

        /// <summary>
        /// 入库中
        /// </summary>
        Inbounding = 2,

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
