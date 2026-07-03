using System;

namespace TuTa.Wms.Stocks.Events
{
    public class StockBindBoxEvent
    {
        public Guid BoxId { get; set; }

        public Guid StockId { get; set; }

        public string StockBarcode { get; set; }


    }
}
