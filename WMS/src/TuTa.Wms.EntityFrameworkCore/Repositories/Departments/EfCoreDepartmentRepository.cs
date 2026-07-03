using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Departments;
using TuTa.Wms.Departments.Aggregates;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Departments
{
    internal class EfCoreDepartmentRepository : EfCoreRepository<WmsDbContext, Department, Guid>, IDepartmentRepository
    {
        public EfCoreDepartmentRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Department> FindByIdAsync(
            Guid id,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Department> FindByNameAsync(
            string name, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.DepartmentName == name, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Department> FindByCodeAsync(
            string code, bool 
            isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.DepartmentCode == code, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Department>> GetAllAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .OrderBy(o => o.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
