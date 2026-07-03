using System;
using System.Collections.Generic;

namespace TuTa.Wms.Cells.Dtos
{
    public class CellsBindAreaDto
    {
        public Guid WarehouseId { get; set; }

        public int WarehouseAreaId { get; set; }

        public List<Guid> CellIds { get; set; }
    }
}
