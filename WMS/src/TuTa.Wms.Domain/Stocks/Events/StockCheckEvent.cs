using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using Volo.Abp;

namespace TuTa.Wms.Stocks.Events
{
    public class StockCheckEvent
    {
        public StockCheckEvent(
            string barcode,
            string boxcode,
            decimal count)
        {
            Barcode = barcode;
            Boxcode = boxcode;
            Count = count;
        }

        public string Barcode { get; }
        public string Boxcode { get; }
        public decimal Count { get; }
    }
}
