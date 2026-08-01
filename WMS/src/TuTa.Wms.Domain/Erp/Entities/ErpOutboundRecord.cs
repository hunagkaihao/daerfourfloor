using System;
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
    }
}
