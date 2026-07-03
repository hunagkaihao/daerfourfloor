using System;

namespace TuTa.Wms.Stocks.Events
{
    public class StockUsedUpEvent
    {
        public Guid StockId { get; set; }

        public Guid? BoxId { get; set; }
    }
}
