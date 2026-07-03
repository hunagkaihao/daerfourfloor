using System;

namespace TuTa.Wms.Cells
{
    public class CellCreatedEvent
    {
        public Guid CellId { get; set; }

        public Guid WarehouseId { get; set; }

        public int? WarehouseAreaId { get; set; }

        public string ShelfName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public CellType CellType { get; set; }

        public string AvailableBoxSpecsNames { get; set; }
    }
}
