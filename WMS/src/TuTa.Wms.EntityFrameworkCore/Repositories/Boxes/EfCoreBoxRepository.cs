using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Boxes
{
    public class EfCoreBoxRepository : EfCoreRepository<WmsDbContext, Box, Guid>, IBoxRepository
    {
        public EfCoreBoxRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Box> FindByBoxCodeAsync(
            string boxCode,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.BoxCode == boxCode)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Box> FindByBoxIdAsync(
            Guid boxId,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.Id == boxId)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Box> FindByBoxNameAsync(
            string boxName,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.BoxName == boxName)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<List<Box>> GetByCellsIdAsync(
            List<Guid> cellIds,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => cellIds.Contains((Guid)o.CellData.CellId))
                .ToListAsync();
        }
        public async Task<List<Box>> GetNoHaveByCellsIdAsync(
            List<Guid> cellIds,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => cellIds.Contains((Guid)o.CellData.CellId) && o.Status == BoxStatus.NoHave)
                .ToListAsync();
        }

        public async Task<Box> FindByCellIdAsync(
            Guid cellId,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.CellData.CellId == cellId)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Box>> GetAllAsync(
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .OrderByDescending(o => o.CreationTime)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Box>> GetNoHaveInAsync(
            int count,string type,List<string> cellcellCodes,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(t => t.WarehouseData != null && t.WarehouseData.WarehouseAreaId == 1 && t.Status == BoxStatus.NoHave && t.BoxTypeName == type && cellcellCodes.Contains(t.CellData.CellCode))
                .OrderBy(t => t.CellData.CellCode)
                .Take(count)
                .IncludeIf(includeDetails, o => o.StocksInBox)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QueryDataInPage<Box>> GetPagedBoxAsync(
            string boxCode, 
            string boxName, 
            Guid? cellId,
            int? warehouseAreaId,
            Guid? warehouseId,
            bool isTrack = true,
            bool includeDetails = true, 
            int skipCount = 0, 
            int maxResultCount = 10, 
            CancellationToken cancellationToken = default)
        {
            DbSet<Box> dbSet = await GetDbSetAsync();
            IQueryable<Box> querable = dbSet
                .TrackIf(isTrack)
                .Where(o => 
                (boxCode == null ? true : o.BoxCode.Contains(boxCode)) &&
                (boxName == null ? true : o.BoxName.Contains(boxName)) &&
                (cellId == null ? true : o.CellData.CellId == cellId) &&
                (warehouseAreaId == null ? true : o.WarehouseData.WarehouseAreaId == warehouseAreaId) &&
                (warehouseId == null ? true : o.WarehouseData.WarehouseId == warehouseId));
            
            return new QueryDataInPage<Box>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    .IncludeIf(includeDetails, o => o.StocksInBox)
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }
    }
}
