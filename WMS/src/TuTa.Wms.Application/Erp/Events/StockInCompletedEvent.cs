using System;

namespace TuTa.Wms.Erp.Events
{
    public class StockInCompletedEvent
    {
        public string AsnCode { get; }
        
        public decimal StockInQuantity { get; }
        
        public string Barcode { get; }
        
        public DateTime EventTime { get; }

        public StockInCompletedEvent(string asnCode, decimal stockInQuantity, string barcode = null)
        {
            AsnCode = asnCode;
            StockInQuantity = stockInQuantity;
            Barcode = barcode;
            EventTime = DateTime.Now;
        }
    }
}