using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Cells
{
    public class EfCoreCellRepository : EfCoreRepository<WmsDbContext, Cell, Guid>, ICellRepository
    {
        public EfCoreCellRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Cell> FindByCellCodeAsync(
            string cellCode,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .FirstOrDefaultAsync(o => o.CellCode == cellCode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Cell> FindByCellCode2Async(
            string cellCode,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .FirstOrDefaultAsync(o => o.CellCode2 == cellCode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Cell> FindByCellNameAsync(
            string cellName, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .FirstOrDefaultAsync(o => o.CellName == cellName, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Cell> FindByIdAsync(
            Guid cellId,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .FirstOrDefaultAsync(o => o.Id == cellId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> FindCountByShelfNameAsync(
            string shelfName,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(t => t.ShelfName == shelfName && t.CellStatus != CellStatus.Nohave).CountAsync();
        }

        public async Task<List<Cell>> FindByZhouZhuanAsync(
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o=>o.WarehouseAreaId==4 && o.CellType==CellType.Skip)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindByAreaTypeAvailableAsync(
            int areaId,CellType type,string ava,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o => o.WarehouseAreaId == areaId && o.CellType == type && o.AvailableSkipSpecsNames==ava)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindByZhouZhuanCellAsync(
            List<string> skips,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o => skips.Contains(o.ShelfName) && o.RunStatus == CellRunStatus.Enable)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindBySkipCellAsync(
            string skip,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o => o.ShelfName == skip)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindByAreaCellAsync(
            int areaId,int count,string ava,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(t => t.WarehouseAreaId == areaId && t.CellType == CellType.CTUCell
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.AvailableBoxSpecsNames == ava)
                .Take(count)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindByWorkSendAsync(
            int areaId, string boxtype,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(t => t.WarehouseAreaId == areaId
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.CellType == CellType.Skip
                    && t.AvailableBoxSpecsNames == boxtype)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> FindSkipCellByAreaTypeAsync(
            int areaId,int skipType,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(t => t.WarehouseAreaId == areaId && t.CellType == CellType.Skip 
                    && (t.AvailableSkipSpecsNames == skipType.ToString())
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        //public async Task<List<Cell>> GetNoHaveByWall(
        //    int count,
        //    bool includeDetails = true,
        //    CancellationToken cancellationToken = default)
        //{
        //    var dbSet = await GetDbSetAsync().ConfigureAwait(false);
        //    return await dbSet
        //        .IncludeIf(includeDetails, o => o.CellBoxes)
        //        .Where(t => t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell
        //            && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave)
        //        .Take(count)
        //        .ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);
        //}

        public async Task<List<Cell>> GetNoHaveByAreaCellType(
            int count, int areaId, CellType type,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(t => t.WarehouseAreaId == areaId && t.CellType == type
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave)
                .Take(count)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> GetNoHaveBox(
            string ava,CellType type,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .Where(t => t.WarehouseAreaId == 1 && t.CellType == type && t.AvailableBoxSpecsNames==ava)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Cell>> GetNoHaveBoxWall(
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .Where(t => t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell && t.CellStatus != CellStatus.Nohave && t.RunStatus ==CellRunStatus.Enable)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<QueryDataInPage<Cell>> GetPagedCellsAsync(
            Guid? warehouseId, 
            int? warehouseAreaId, 
            string shelfName, 
            CellStatus? cellStatus, 
            CellRunStatus? cellRunStatus, 
            CellType? cellType, 
            string availableBoxSpecsNamesTip, 
            string cellCodeTip, 
            string cellNameTip,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Cell> queryable = dbSet
                .AsNoTracking()
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o => 
                (warehouseId == null ? true : o.WarehouseId == warehouseId) &&
                (warehouseAreaId == null ? true : o.WarehouseAreaId == warehouseAreaId) &&
                (shelfName == null ? true : o.ShelfName == shelfName) &&
                (cellStatus == null ? true : o.CellStatus == cellStatus) &&
                (cellType == null ? true : o.CellType == cellType) &&
                (cellRunStatus == null ? true : o.RunStatus == cellRunStatus) &&
                (availableBoxSpecsNamesTip == null ? true : o.AvailableBoxSpecsNames.Contains(availableBoxSpecsNamesTip)) &&
                (cellCodeTip == null ? true : o.CellCode.Contains(cellCodeTip)) &&
                (cellNameTip == null ? true : o.CellName.Contains(cellNameTip)));

            return new QueryDataInPage<Cell>()
            {
                TotalCount = await queryable.CountAsync().ConfigureAwait(false),
                Items = await queryable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<QueryDataInPage<Cell>> GetPagedCellsByAreaAsync(
            int warehouseAreaId,
            string heigh,
            string weight,
            CellType cellType,
            string cellCode,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Cell> queryable = dbSet
                .AsNoTracking()
                .IncludeIf(includeDetails, o => o.CellBoxes)
                .Where(o =>o.WarehouseAreaId == warehouseAreaId &&
                (o.CellHeight == heigh) &&
                (o.CellWeight == weight) &&
                (o.CellType == cellType) &&
                (cellCode == null ? true : o.CellCode.Contains(cellCode)));

            return new QueryDataInPage<Cell>()
            {
                TotalCount = await queryable.CountAsync().ConfigureAwait(false),
                Items = await queryable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<List<string>> GetCTUAreaAvaAsync(
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return dbSet.Where(t => t.WarehouseAreaId == 1).Select(t => t.AvailableBoxSpecsNames).Distinct().ToList();
        }
    }
}
