using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.StockInHistories.Dtos
{
    public class StockInHistoryDto : AuditedEntityDto<int>
    {
        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string MaterialSpecs { get; set; }

        public string MaterialUnit { get; set; }


        public string StockInType { get; set; }

        public decimal InCount { get; set; }

        public DateTime InTime { get; set; }

        public string OperatorName { get; set; }


        public string Barcode { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }


        public string CheckOrderCode { get; set; }

        public string CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        public string CheckResult { get; set; }


        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public string BatchNo { get; set; }

        public bool IsHB { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string BLCode { get; set; }

        public string BHCode { get; set; }
    }
}
