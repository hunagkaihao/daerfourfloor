using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.StockInHistories.Dtos
{
    public class PagedStockInHistoryQueryDto
    {
        public string Barcode { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string StockInType { get; set; }

        public DateTime? StockInTimeStart { get; set; }

        public DateTime? StockInTimeEnd { get; set; }

        public string CheckNoTip { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
