using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Stocks.Dtos
{
    public class StockCheckDto
    {
        public string Barcode {  get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public int count { get; set; }
    }
}
