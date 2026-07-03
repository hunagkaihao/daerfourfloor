using System;

namespace TuTa.Wms.PickLists.Dtos
{
    public class NoPlanPickListEditDto
    {
        public Guid NoPlanPickListIdToEdit { get; set; }

        public string UniqueCodeToEdit { get; set; }

        public int NewPickType { get; set; }

        public decimal NewPickCount { get; set; }

        public string NewPickerName { get; set; }
    }
}
