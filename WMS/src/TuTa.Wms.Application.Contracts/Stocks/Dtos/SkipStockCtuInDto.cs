using System.Collections.Generic;

namespace TuTa.Wms.Stocks.Dtos
{
    public class SkipStockCtuInDto
    {
        public string SkipCode { get; set; }

        public List<StocksCtuInDto> SkipStocksCtuIn { get; set; }

    }

    public class StocksCtuInDto
    {
        public string BoxCode { get; set; }

        public string StartCode { get; set; }

        public string EndCode { get; set; }
    }
}
