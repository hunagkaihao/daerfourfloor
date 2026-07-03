using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Departments;
using TuTa.Wms.Departments.Dtos;

namespace TuTa.Wms.Controllers.Boxes;

[Route("wms/department")]
[ApiController]
public class DepartmentController : WmsController, IDepartmentService
{
    private readonly IDepartmentService _departmentService;
    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("allDepartmentsGet")]
    public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
    {
        return await _departmentService.GetAllDepartmentsAsync().ConfigureAwait(false);
    }


    [HttpPost("createDept")]
    public async Task<ResponseDto> CreateDepartment(DepartmentDto para)
    {
        return await _departmentService.CreateDepartment(para).ConfigureAwait(false);
    }
}