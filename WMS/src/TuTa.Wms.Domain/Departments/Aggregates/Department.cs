using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Departments.Aggregates
{
    public class Department : AuditedAggregateRoot<Guid>
    {
        private Department()
        {            
        }

        internal Department(Guid id, string departmentCode, string departmentName)
            :base(id)
        {
            DepartmentCode = Check.NotNullOrWhiteSpace(departmentCode, nameof(departmentCode));
            DepartmentName = Check.NotNullOrWhiteSpace(departmentName, nameof(departmentName));
        }

        [StringLength(30)]
        [Required]
        public string DepartmentCode { get; private set; }

        [StringLength(60)]
        [Required]
        public string DepartmentName { get; private set; }
    }
}
