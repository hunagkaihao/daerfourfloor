using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.StockOutHistories.Dtos
{
    public class StockOutHistoryDto : AuditedEntityDto<int>
    {
        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string Specs { get; set; }

        public string Unit { get; set; }


        public string StockOutType { get; set; }

        public decimal StockOutCount { get; set; }

        public string PickBatch { get; set; }

        public string UniqueCode { get; set; }

        public DateTime StockOutTime { get; set; }

        public string Operator { get; set; }


        public string Barcode { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public string HouseCode { get; set; }

        public string HouseName { get; set; }


        public string CheckOrderCode { get; set; }

        public string CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        public string CheckResult { get; set; }


        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }


        public string DeptCode { get; set; }

        public string DeptName { get; set; }

        public string GysCode { get; set; }

        public string GysName { get; set; }


        public string GoodsCode { get; set; }

        public string GoodsName { get; set; }

        public string GoodsSpecs { get; set; }
    }
}
