using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Aggregates;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.SKips
{
    public class EfCoreSkipRepository : EfCoreRepository<WmsDbContext, Skip, Guid>, ISkipRepository
    {
        public EfCoreSkipRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Skip> FindBySkipCodeAsync(string skipCode,bool isTrack = true, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var dbSet=await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o=>o.SkipCode == skipCode,cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Skip> FindByCellIdAsync(Guid? cellId, bool isTrack = true, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.CellId == cellId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Skip>> FindInZhouZhuanAsync(List<Guid> Ids,int type,bool isTrack = true, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => Ids.Contains((Guid)o.CellId) && o.Type == type)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QueryDataInPage<Skip>> GetPagedSkipsAsync(
            int warehouseAreaId,
            SkipStatus status,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Skip> queryable = dbSet
                .AsNoTracking()
                .Where(o =>
                (o.AreaId == warehouseAreaId) &&
                (o.SkipStatus == status));

            return new QueryDataInPage<Skip>()
            {
                TotalCount = await queryable.CountAsync().ConfigureAwait(false),
                Items = await queryable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }
    }
}
