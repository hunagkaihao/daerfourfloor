using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.RecheckLists
{
    public class EfCoreRecheckListRepository : EfCoreRepository<WmsDbContext, RecheckList, Guid>, IRecheckListRepository
    {
        public EfCoreRecheckListRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<RecheckList> FindByReCheckListCodeAsync(
            string reCheckListCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.RecheckItems)
                    .ThenInclude(o => o.RecheckStocks)
                    .FirstOrDefaultAsync(o => o.RecheckListCode == reCheckListCode, cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .FirstOrDefaultAsync(o => o.RecheckListCode == reCheckListCode, cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<RecheckList>> GetAllRecheckListsAsync(
            bool isTrack = true, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.RecheckItems)
                    .ThenInclude(o => o.RecheckStocks)
                    .OrderByDescending(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .OrderByDescending(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
