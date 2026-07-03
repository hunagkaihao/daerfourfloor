using System;
using System.Collections.Generic;

namespace TuTa.Wms.Application.Contracts.Erp.IDto
{
    public class ErpDeliveryOrderDto
    {
        public Guid Id { get; set; }

        public string DeliveryOrderNo { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string Status { get; set; }

        public DateTime? CompletedTime { get; set; }

        public string Remarks { get; set; }

        public DateTime CreationTime { get; set; }

        public List<ErpDeliveryOrderItemDto> Items { get; set; } = new List<ErpDeliveryOrderItemDto>();
    }

    public class ErpDeliveryOrderItemDto
    {
        public Guid Id { get; set; }

        public Guid DeliveryOrderId { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string Specs { get; set; }

        public string Unit { get; set; }

        public decimal DeliveryQuantity { get; set; }

        public string BatchCode { get; set; }

        public string BoxNo { get; set; }

        public string Packaging { get; set; }

        public string Grade { get; set; }

        public string LabelPrint { get; set; }

        public decimal QuantityPerBox { get; set; }

        public decimal ShippedQuantity { get; set; }
    }

    public class ErpDeliveryOrderCreateDto
    {
        public string DeliveryOrderNo { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string Remarks { get; set; }

        public List<ErpDeliveryOrderItemCreateDto> Items { get; set; } = new List<ErpDeliveryOrderItemCreateDto>();
    }

    public class ErpDeliveryOrderItemCreateDto
    {
        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string Specs { get; set; }

        public string Unit { get; set; }

        public decimal DeliveryQuantity { get; set; }

        public string BatchCode { get; set; }

        public string BoxNo { get; set; }

        public string Packaging { get; set; }

        public string Grade { get; set; }

        public string LabelPrint { get; set; }

        public decimal QuantityPerBox { get; set; }
    }

    public class ErpDeliveryOrderListResponseDto
    {
        public List<ErpDeliveryOrderDto> Items { get; set; } = new List<ErpDeliveryOrderDto>();

        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}