using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Boxes.Dtos
{
    public class PagedBoxesQueryDto
    {
        public string WarehouseName { get; set; }

        public string WarehouseAreaName { get; set; }

        public string CellName { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
