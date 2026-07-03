using System;

namespace TuTa.Wms.Boxes.Events
{
    public class BoxDisBindCellEvent
    {
        public Guid BoxId { get; set; }

        public Guid CellId { get; set; }
    }
}
