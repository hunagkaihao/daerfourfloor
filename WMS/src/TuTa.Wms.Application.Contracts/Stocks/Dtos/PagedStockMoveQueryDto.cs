using System;
using TuTa.Wms.ChkResultLists;

namespace TuTa.Wms.Stocks.Dtos
{
    public class PagedStockMoveQueryDto
    {
        public int AreaId{ get; set; }

        public string MaterialName { get; set; }

        public string CheckNo { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
