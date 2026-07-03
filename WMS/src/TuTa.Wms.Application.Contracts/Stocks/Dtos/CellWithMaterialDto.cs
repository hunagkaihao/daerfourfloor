using System;

namespace TuTa.Wms.Stocks.Dtos
{
    public class CellWithMaterialDto
    {
        public Guid CellId { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public string StockInDate { get; set; }

        public decimal StockCount { get; set; }

        public string CheckNo { get; set;}

        public string BoxCode { get; set; }

        public string Barcode { get; set; }
    }
}
