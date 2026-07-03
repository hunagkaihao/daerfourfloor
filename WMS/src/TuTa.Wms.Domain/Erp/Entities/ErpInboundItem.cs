using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    /// <summary>
    /// ERP入库单项实体
    /// </summary>
    public class ErpInboundItem : Entity<int>
    {
        private ErpInboundItem()
        {
        }

        internal ErpInboundItem(
            Guid inboundOrderId,
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

            InboundOrderId = inboundOrderId;
            MaterialCode = materialCode;
            MaterialName = materialName;
            PlanInboundQty = planInboundQty;
            ActualInboundQty = actualInboundQty;
            UnitCode = unitCode;
            MoNo = moNo;
            LevelCode = levelCode;
            LotNo = lotNo;
            Specs = specs;
            Status = InboundItemStatus.Created;
        }

        /// <summary>
        /// 修改入库单项
        /// </summary>
        internal void Modify(
            string materialName,
            decimal planInboundQty,
            decimal actualInboundQty,
            string unitCode,
            string moNo = null,
            string levelCode = null,
            string lotNo = null,
            string specs = null)
        {
            if (Status != InboundItemStatus.Created)
                throw new Exception("当前入库项已经在入库中或已完成，不能进行修改");

            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.Positive(planInboundQty, nameof(planInboundQty));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));

            MaterialName = materialName;
            PlanInboundQty = planInboundQty;
            ActualInboundQty = actualInboundQty;
            UnitCode = unitCode;
            MoNo = moNo;
            LevelCode = levelCode;
            LotNo = lotNo;
            Specs = specs;
        }

        /// <summary>
        /// 设置入库项状态
        /// </summary>
        internal void SetStatus(InboundItemStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// 更新实际入库数量
        /// </summary>
        internal void UpdateActualInboundQty(decimal actualInboundQty)
        {
            if (actualInboundQty < 0)
                throw new Exception("实际入库数量不能为负数");

            if (actualInboundQty > PlanInboundQty)
                throw new Exception("实际入库数量不能超过计划入库数量");

            ActualInboundQty = actualInboundQty;

            // 根据实际入库数量更新状态
            if (actualInboundQty == 0)
                Status = InboundItemStatus.Created;
            else if (actualInboundQty < PlanInboundQty)
                Status = InboundItemStatus.Inbounding;
            else
                Status = InboundItemStatus.Completed;
        }

        /// <summary>
        /// 入库单ID（外键）
        /// </summary>
        [Required]
        public virtual Guid InboundOrderId { get; private set; }

        /// <summary>
        /// 材料代号
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string MaterialCode { get; private set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [StringLength(100)]
        [Required]
        public virtual string MaterialName { get; private set; }

        /// <summary>
        /// 计划入库数量
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public virtual decimal PlanInboundQty { get; private set; }

        /// <summary>
        /// 实际入库数量
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public virtual decimal ActualInboundQty { get; private set; }

        /// <summary>
        /// 单位
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string UnitCode { get; private set; }

        /// <summary>
        /// 制令号
        /// </summary>
        [StringLength(50)]
        public virtual string MoNo { get; private set; }

        /// <summary>
        /// 等级代号
        /// </summary>
        [StringLength(20)]
        public virtual string LevelCode { get; private set; }

        /// <summary>
        /// 批号
        /// </summary>
        [StringLength(50)]
        public virtual string LotNo { get; private set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        [StringLength(200)]
        public virtual string Specs { get; private set; }

        /// <summary>
        /// 入库项状态
        /// </summary>
        [Required]
        public virtual InboundItemStatus Status { get; private set; }
    }

    /// <summary>
    /// 入库项状态枚举
    /// </summary>
    public enum InboundItemStatus
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
