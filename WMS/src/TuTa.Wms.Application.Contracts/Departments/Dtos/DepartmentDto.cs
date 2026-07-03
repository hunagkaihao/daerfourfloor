using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Departments.Dtos
{
    public class DepartmentDto : EntityDto<Guid>
    {
        public string DepartmentCode { get; set; }

        public string DepartmentName { get; set; }
    }
}
