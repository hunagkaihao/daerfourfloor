using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Departments.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Departments
{
    public interface IDepartmentRepository : IRepository<Department, Guid>
    {
        Task<Department> FindByIdAsync(
            Guid id, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default);

        Task<Department> FindByNameAsync(
            string name,
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        Task<Department> FindByCodeAsync(
            string code,
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        Task<List<Department>> GetAllAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
