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
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.PickLists
{
    public class EfCorePickListRepository : EfCoreRepository<WmsDbContext, PickList, Guid>, IPickListRepository
    {
        public EfCorePickListRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
        public async Task<PickList> FindByPickListIdAsync(
            Guid pickListId,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .FirstOrDefaultAsync(o => o.Id == pickListId, cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .FirstOrDefaultAsync(o => o.Id == pickListId, cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<PickList> FindByPickListCodeAsync(
            string pickListCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .FirstOrDefaultAsync(o => o.PickListCode == pickListCode, cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .FirstOrDefaultAsync(o => o.PickListCode == pickListCode, cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<PickList>> GetPickListsByPickTypeAsync(
            int type,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();

            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .Where(o => o.Type == type)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .Where(o => o.Type == type)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<PickList>> GetPickListsByDepartmentCodeAsync(
            string departmentCode, 
            bool isTrack = true, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();

            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .Where(o => (departmentCode == null ? true : o.Picker.DeptCode == departmentCode))
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .Where(o => (departmentCode == null ? true : o.Picker.DeptCode == departmentCode))
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<PickList>> GetPickListsByPickTypeAndPickBatchAsync(
            int type,
            string pickBatchTip,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();

            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .Where(o => o.Type == type && o.PickBatch.Contains(pickBatchTip))
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .Where(o => o.Type == type && o.PickBatch.Contains(pickBatchTip))
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<PickList>> GetAllPickListsAsync(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();

            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task<List<PickList>> GetAllUnFinishedPickListsAsync(
            bool isTrack = true, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();

            if (includeDetails)
                return await dbSet
                    .TrackIf(isTrack)
                    .Where(o => o.Status == PickOrderStatus.Created || o.Status == PickOrderStatus.Picking)
                    .Include(o => o.PickItems)
                    .ThenInclude(o => o.PickStocks)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            else
                return await dbSet
                    .TrackIf(isTrack)
                    .Where(o => o.Status == PickOrderStatus.Created || o.Status == PickOrderStatus.Picking)
                    .OrderBy(o => o.CreationTime)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
