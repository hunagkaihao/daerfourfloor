using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Departments.Aggregates;
using TuTa.Wms.Departments.Dtos;
using Volo.Abp;
using Wms.LogTool;

namespace TuTa.Wms.Departments
{
    public class DepartmentService : WmsAppService, IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly DepartmentManager _manager;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(
            IDepartmentRepository departmentRepository,
            DepartmentManager manager,
            ILogger<DepartmentService> logger)
        {
            _departmentRepository = departmentRepository;
            _manager = manager;
            _logger = logger;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            try
            {
                var dpts =  await _departmentRepository.GetAllAsync(false).ConfigureAwait(false);
                if (dpts == null || dpts.Count == 0) return new List<DepartmentDto>();
                
                return ObjectMapper.Map<List<Department>, List<DepartmentDto>>(dpts);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> CreateDepartment(DepartmentDto para)
        {
            try
            {
                ResponseDto dto = new ResponseDto();
                var dpts = await _manager.CreateDepartmentAsync(para.DepartmentCode,para.DepartmentName).ConfigureAwait(false);
                await _departmentRepository.InsertAsync(dpts);
                dto.success = true;
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
