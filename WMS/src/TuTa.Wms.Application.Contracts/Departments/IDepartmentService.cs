using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Departments.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Departments
{
    public interface IDepartmentService : IApplicationService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();

        Task<ResponseDto> CreateDepartment(DepartmentDto department);
    }

}
