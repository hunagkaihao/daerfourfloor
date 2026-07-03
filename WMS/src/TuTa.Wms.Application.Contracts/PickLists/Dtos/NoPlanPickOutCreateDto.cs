using System;

namespace TuTa.Wms.PickLists.Dtos
{
    public class NoPlanPickOutCreateDto
    {
        public Guid DepartmentId { get; set; }

        public string MaterialCode { get; set; }

        public int PickType { get; set; }

        public decimal PickCount { get; set; }

        public string PickerName { get; set;}
    }
}
