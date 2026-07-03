using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.PickLists;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Entities;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.PickLists
{
    public class EfCorePickItemRepository : EfCoreRepository<WmsDbContext, PickItem, int>, IPickItemRepository
    {
        public EfCorePickItemRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<PickItem>> GetByMaterial(
            string materialCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.MaterialCode == materialCode && o.Status != PickItemStatus.Picked)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<PickItem> GetByUnique(
            string unique,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.UniqueCode == unique)
                .ConfigureAwait(false);
        }
    }
}
