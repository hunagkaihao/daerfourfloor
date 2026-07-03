using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.StockOutHistories.Dtos
{
    public class PagedStockOutHistoryQueryDto
    {
        public string Barcode { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string StockOutType { get; set; }

        public DateTime? StockOutTimeMin { get; set; }

        public DateTime? StockOutTimeMax { get; set; }

        public string CheckNoTip { get; set; }

        public string PickBatchTip { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
