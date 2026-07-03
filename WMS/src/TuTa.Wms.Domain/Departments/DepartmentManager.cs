using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.Departments.Aggregates;

namespace TuTa.Wms.Departments
{
    public class DepartmentManager : WmsDomainService
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentManager(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<Department> CreateDepartmentAsync(string departmentCode, string departmentName)
        {
            var departmentExist = await _repository.FindByCodeAsync(departmentCode).ConfigureAwait(false);
            if (departmentExist != null)
                throw new Exception($"部门码为{departmentCode}的部门已存在");

            departmentExist = await _repository.FindByNameAsync(departmentName).ConfigureAwait(false);
            if (departmentExist != null)
                throw new Exception($"部门名为{departmentName}的部门已存在");

            return new Department(GuidGenerator.Create(), departmentCode, departmentName);
        }
    }
}
