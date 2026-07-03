using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.PickLists.Dtos
{
    public class GetByBarcodeBoxDto
    {
        public PickItemDto PickDto { get; set; }

        public List<PickItemDto> Items { get; set; }
    }
}
