using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Aggregates;
using TuTa.Wms.RecheckLists.Entities;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.RecheckLists
{
    public class EfCoreRecheckItemRepository : EfCoreRepository<WmsDbContext, RecheckItem, int>, IRecheckItemRepository
    {
        public EfCoreRecheckItemRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<RecheckItem> FindByCheckNoAsync(
            string checkNo,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
                return await dbSet
                    .TrackIf(isTrack)
                    .FirstOrDefaultAsync(o => o.CheckNo == checkNo, cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
