using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpDepartmentRepository : EfCoreRepository<ErpDbContext, ErpDepartment>, IErpDepartmentRepository
    {
        public EfCoreErpDepartmentRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpDepartment>> GetAllErpDepartmentsAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.TrackIf(isTrack).OrderBy(o => o.DEPT_ID).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
