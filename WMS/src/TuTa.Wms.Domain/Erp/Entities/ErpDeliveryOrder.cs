using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpDeliveryOrder : AuditedAggregateRoot<Guid>
    {
        private ErpDeliveryOrder() { }

        public static ErpDeliveryOrder Create(
            Guid id,
            string deliveryOrderNo,
            string warehouseCode,
            string warehouseName,
            DateTime deliveryDate,
            string status = null)
        {
            Check.NotNullOrWhiteSpace(deliveryOrderNo, nameof(deliveryOrderNo));
            Check.NotNullOrWhiteSpace(warehouseCode, nameof(warehouseCode));

            return new ErpDeliveryOrder
            {
                Id = id,
                DeliveryOrderNo = deliveryOrderNo,
                WarehouseCode = warehouseCode,
                WarehouseName = warehouseName,
                DeliveryDate = deliveryDate,
                Status = status ?? DeliveryOrderStatus.Created
            };
        }

        [StringLength(50)]
        [Required]
        public virtual string DeliveryOrderNo { get; private set; }

        [StringLength(20)]
        [Required]
        public virtual string WarehouseCode { get; private set; }

        [StringLength(100)]
        public virtual string WarehouseName { get; private set; }

        public virtual DateTime DeliveryDate { get; private set; }

        [StringLength(20)]
        public virtual string Status { get; private set; }

        public virtual DateTime? CompletedTime { get; private set; }

        [StringLength(500)]
        public virtual string Remarks { get; private set; }

        public void SetStatus(string status)
        {
            Status = status;
            if (status == DeliveryOrderStatus.Completed)
            {
                CompletedTime = DateTime.Now;
            }
        }

        public void UpdateRemarks(string remarks)
        {
            Remarks = remarks;
        }
    }

    public class ErpDeliveryOrderItem : AuditedAggregateRoot<Guid>
    {
        private ErpDeliveryOrderItem() { }

        public static ErpDeliveryOrderItem Create(
            Guid id,
            Guid deliveryOrderId,
            string materialCode,
            string materialName,
            string specs,
            string unit,
            decimal deliveryQuantity,
            string batchCode,
            string boxNo = null,
            string packaging = null,
            string grade = null,
            string labelPrint = null,
            decimal quantityPerBox = 0)
        {
            Check.NotNull(deliveryOrderId, nameof(deliveryOrderId));
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));

            return new ErpDeliveryOrderItem
            {
                Id = id,
                DeliveryOrderId = deliveryOrderId,
                MaterialCode = materialCode,
                MaterialName = materialName,
                Specs = specs,
                Unit = unit,
                DeliveryQuantity = deliveryQuantity,
                BatchCode = batchCode,
                BoxNo = boxNo,
                Packaging = packaging,
                Grade = grade,
                LabelPrint = labelPrint,
                QuantityPerBox = quantityPerBox,
                ShippedQuantity = 0
            };
        }

        public virtual Guid DeliveryOrderId { get; private set; }

        [StringLength(50)]
        [Required]
        public virtual string MaterialCode { get; private set; }

        [StringLength(200)]
        public virtual string MaterialName { get; private set; }

        [StringLength(200)]
        public virtual string Specs { get; private set; }

        [StringLength(10)]
        public virtual string Unit { get; private set; }

        public virtual decimal DeliveryQuantity { get; private set; }

        [StringLength(50)]
        public virtual string BatchCode { get; private set; }

        [StringLength(50)]
        public virtual string BoxNo { get; private set; }

        [StringLength(50)]
        public virtual string Packaging { get; private set; }

        [StringLength(20)]
        public virtual string Grade { get; private set; }

        [StringLength(200)]
        public virtual string LabelPrint { get; private set; }

        public virtual decimal QuantityPerBox { get; private set; }

        public virtual decimal ShippedQuantity { get; private set; }

        public void AddShippedQuantity(decimal quantity)
        {
            ShippedQuantity += quantity;
        }

        public bool IsShippedCompleted()
        {
            return ShippedQuantity >= DeliveryQuantity;
        }
    }

    public static class DeliveryOrderStatus
    {
        public const string Created = "Created";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}