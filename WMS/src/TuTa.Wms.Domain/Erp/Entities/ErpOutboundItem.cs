using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    /// <summary>
    /// ERP出库单项实体
    /// </summary>
    public class ErpOutboundItem : Entity<int>
    {
        private ErpOutboundItem()
        {
        }

        internal ErpOutboundItem(
            Guid outboundOrderId,
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

            OutboundOrderId = outboundOrderId;
            MaterialCode = materialCode;
            MaterialName = materialName;
            PlanOutboundQty = planOutboundQty;
            ActualOutboundQty = actualOutboundQty;
            UnitCode = unitCode;
            MoNo = moNo;
            LevelCode = levelCode;
            LotNo = lotNo;
            PlaceCode = placeCode;
            DeliveryCode = deliveryCode;
            Status = OutboundItemStatus.Created;
        }

        /// <summary>
        /// 修改出库单项
        /// </summary>
        internal void Modify(
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
            if (Status != OutboundItemStatus.Created)
                throw new Exception("当前出库项已经在出库中或已完成，不能进行修改");

            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.Positive(planOutboundQty, nameof(planOutboundQty));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));

            MaterialName = materialName;
            PlanOutboundQty = planOutboundQty;
            ActualOutboundQty = actualOutboundQty;
            UnitCode = unitCode;
            MoNo = moNo;
            LevelCode = levelCode;
            LotNo = lotNo;
            PlaceCode = placeCode;
            DeliveryCode = deliveryCode;
        }

        /// <summary>
        /// 设置出库项状态
        /// </summary>
        internal void SetStatus(OutboundItemStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// 更新实际出库数量
        /// </summary>
        internal void UpdateActualOutboundQty(decimal actualOutboundQty)
        {
            if (actualOutboundQty < 0)
                throw new Exception("实际出库数量不能为负数");

            if (actualOutboundQty > PlanOutboundQty)
                throw new Exception("实际出库数量不能超过计划出库数量");

            ActualOutboundQty = actualOutboundQty;

            // 根据实际出库数量更新状态
            if (actualOutboundQty == 0)
                Status = OutboundItemStatus.Created;
            else if (actualOutboundQty < PlanOutboundQty)
                Status = OutboundItemStatus.Outbounding;
            else
                Status = OutboundItemStatus.Completed;
        }

        /// <summary>
        /// 出库单ID（外键）
        /// </summary>
        [Required]
        public virtual Guid OutboundOrderId { get; private set; }

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
        /// 计划出库数量
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public virtual decimal PlanOutboundQty { get; private set; }

        /// <summary>
        /// 实际出库数量
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public virtual decimal ActualOutboundQty { get; private set; }

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
        /// 储位代号
        /// </summary>
        [StringLength(50)]
        public virtual string PlaceCode { get; private set; }

        /// <summary>
        /// 配送位置代号
        /// </summary>
        [StringLength(50)]
        public virtual string DeliveryCode { get; private set; }

        /// <summary>
        /// 出库项状态
        /// </summary>
        [Required]
        public virtual OutboundItemStatus Status { get; private set; }
    }

    /// <summary>
    /// 出库项状态枚举
    /// </summary>
    public enum OutboundItemStatus
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
