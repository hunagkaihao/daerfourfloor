using System;
using TuTa.Wms.ChkResultLists;

namespace TuTa.Wms.Stocks.Dtos
{
    public class StockQueryDto
    {
        public string BoxCode { get; set; }

        public string CellCode { get; set; }

        public string WarehouseAreaName { get; set; }

        public string WarehouseName { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string Barcode { get; set; }

        public StockStatus? Status { get; set; }

        public StockInType? StockInType { get; set; }

        public DateTime? StockInDateStart { get; set; }

        public DateTime? StockInDateEnd { get; set; }

        public EnumCheckType? CheckType { get; set; }

        public EnumCheckResult? CheckResult { get; set; }

        public string CheckNoTip { get; set; }
    }
}
