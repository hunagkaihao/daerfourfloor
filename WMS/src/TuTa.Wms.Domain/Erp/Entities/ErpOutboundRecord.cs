using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpOutboundRecord : Entity<string>
    {
        private ErpOutboundRecord() { }

        public static ErpOutboundRecord Create(
            string warehouse,
            string customerCode,
            string masterId,
            decimal quantity,
            decimal qtyPerBox,
            string materialCode,
            string package,
            string grade,
            string labelText,
            string deliveryOrderNo)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));

            return new ErpOutboundRecord
            {
                Id = Guid.NewGuid().ToString(),
                Warehouse = warehouse,
                CustomerCode = customerCode,
                MasterId = masterId,
                Quantity = quantity,
                QtyPerBox = qtyPerBox,
                MaterialCode = materialCode,
                Package = package,
                Grade = grade,
                LabelText = labelText,
                DeliveryOrderNo = deliveryOrderNo,
                CreationTime = DateTime.Now,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
        }

        public virtual string Warehouse { get; private set; }
        public virtual string CustomerCode { get; private set; }
        public virtual string MasterId { get; private set; }
        public virtual decimal Quantity { get; private set; }
        public virtual decimal QtyPerBox { get; private set; }
        public virtual string MaterialCode { get; private set; }
        public virtual string Package { get; private set; }
        public virtual string Grade { get; private set; }
        public virtual string LabelText { get; private set; }
        public virtual string DeliveryOrderNo { get; private set; }
        public virtual decimal? ActualOutboundQuantity { get; set; }
        public virtual DateTime CreationTime { get; private set; }
        public virtual DateTime? CompletedTime { get; set; }
        public virtual DateTime? OutboundDate { get; set; }
        public virtual DateTime? LastModificationTime { get; set; }
        public virtual string ConcurrencyStamp { get; set; }

        /// <summary>U8发货来源明细ID（iSodid），二维码主表ID不能替代。</summary>
        public virtual long? SourceDetailId { get; private set; }
        /// <summary>销售单部门编码，可在扫码时补充。</summary>
        [StringLength(20)]
        public virtual string SalesDepartmentCode { get; private set; }
        /// <summary>按实际批次记录出库数量的JSON数组，只供U8生单使用。</summary>
        public virtual string ActualOutboundDetailsJson { get; private set; }
        /// <summary>整单销售推送任务UID，重试保持不变。</summary>
        public virtual long? SalesPushTaskId { get; private set; }
        /// <summary>是否已成功生成U8销售出库单。</summary>
        public virtual bool IsSalesPushed { get; private set; }
        /// <summary>U8销售出库推送成功时间。</summary>
        public virtual DateTime? SalesPushTime { get; private set; }
        /// <summary>最近一次销售单推送失败原因。</summary>
        [StringLength(2000)]
        public virtual string SalesPushError { get; private set; }
        /// <summary>仓管员已确认整张发货单的所有物料扫码完成，防止仅部分行录入时提前生单。</summary>
        public virtual bool IsDeliveryScanCompleted { get; private set; }

        /// <summary>确认本地扫描明细已覆盖完整发货单。</summary>
        public void ConfirmDeliveryScan()
        {
            IsDeliveryScanCompleted = true;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        /// <summary>补充U8发货明细来源，已成功生单后禁止修改。</summary>
        public void SetU8SalesSource(long? sourceDetailId, string departmentCode)
        {
            if (IsSalesPushed) throw new BusinessException("销售出库已推送，不能修改来源");
            if (sourceDetailId.HasValue && sourceDetailId <= 0) throw new BusinessException("U8发货明细ID必须大于0");
            SourceDetailId = sourceDetailId;
            SalesDepartmentCode = departmentCode;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        /// <summary>记录实际出库批次数量，数量必须为正且不得超过发货计划。</summary>
        public void RecordActualOutbound(decimal quantity, string detailsJson)
        {
            if (IsSalesPushed || quantity <= 0 || (ActualOutboundQuantity ?? 0) + quantity > Quantity)
                throw new BusinessException("实际出库数量无效或超过U8发货剩余数量");
            ActualOutboundQuantity = (ActualOutboundQuantity ?? 0) + quantity;
            ActualOutboundDetailsJson = detailsJson;
            LastModificationTime = DateTime.Now;
            ConcurrencyStamp = Guid.NewGuid().ToString();
            if (ActualOutboundQuantity == Quantity) CompletedTime = DateTime.Now;
        }

        /// <summary>保存销售单请求UID和推送结果，错误截断以保证落库。</summary>
        public void RecordSalesPush(long uid, bool success, string error)
        {
            SalesPushTaskId = uid;
            IsSalesPushed = success;
            SalesPushTime = success ? DateTime.Now : null;
            SalesPushError = success ? null : error?.Length > 2000 ? error.Substring(0, 2000) : error;
            ConcurrencyStamp = Guid.NewGuid().ToString();
        }
    }
}
