using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpDepartmentRepository : IRepository<ErpDepartment>
    {
        Task<List<ErpDepartment>> GetAllErpDepartmentsAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
