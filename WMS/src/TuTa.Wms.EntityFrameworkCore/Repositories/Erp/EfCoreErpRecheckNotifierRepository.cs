using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class EfCoreErpRecheckNotifierRepository : EfCoreRepository<ErpDbContext, ErpRecheckNotifier>, IErpRecheckNotifierRepository
    {
        public EfCoreErpRecheckNotifierRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpRecheckNotifier>> GetAllUnReceivedRecheckNotifiersAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.IFJS != true)
                .OrderBy(o => o.CKFQTZD_DATE)
                .ThenBy(o => o.CKFQTZD_ID)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<ErpRecheckNotifier>> GetRecheckNotifiersWithNotifierCodeAsync(
            string notifierCode, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.CKFQTZD_ID == notifierCode)
                .OrderBy(o => o.CKFQTZD_DATE)
                .ThenBy(o => o.CKFQTZD_ID)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
