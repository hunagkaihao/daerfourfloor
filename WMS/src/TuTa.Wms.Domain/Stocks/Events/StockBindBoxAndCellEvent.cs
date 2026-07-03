using System;
using TuTa.Wms.ChkResultLists;

namespace TuTa.Wms.Stocks.Events
{
    /// <summary>
    /// 库存同时绑定到容器和库位
    /// </summary>
    public class StockBindBoxAndCellEvent
    {
        public string StockBarcode { get; set; }

        public EnumCheckType CheckType { get; set; }

        public Guid BoxId { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public Guid CellId { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public int? AreaId { get; set; }

        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public Guid HouseId { get; set; }

        public string HouseCode { get; set; }

        public string HouseName { get; set; }

        public decimal StockCount { get; set; }


        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string Specs { get; set; }

        public string Unit { get; set; }


        public string CheckOrderCode { get; set; }

        public DateTime? CheckDate { get; set; }

        public string CheckNo { get; set; }

        public string CheckNoBeforeReCheck { get; set; }

        public string CheckResult { get; set; }

        public string CheckTypeInChs { get; set; }

        public decimal? PassCnt { get; set; }


        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }


        public DateTime StockInDate { get; set; }

        public string StockInType { get; set; }

        public string BatchCode { get; set; }

        public string BLCode { get; set; }

        public string BHCode { get; set; }

        public string Operator { get; set; }
    }
}
