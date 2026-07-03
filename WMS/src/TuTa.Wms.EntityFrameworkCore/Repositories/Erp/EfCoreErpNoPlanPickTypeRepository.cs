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

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpNoPlanPickTypeRepository : EfCoreRepository<ErpDbContext, ErpNoPlanPickType>, IErpNoPlanPickTypeRepository
    {
        public EfCoreErpNoPlanPickTypeRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpNoPlanPickType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.AsNoTracking()
                .OrderBy(o => o.CHKTYPE_ID)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
