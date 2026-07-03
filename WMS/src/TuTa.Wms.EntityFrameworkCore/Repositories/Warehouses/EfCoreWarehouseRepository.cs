using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Aggregates;

namespace Wms.Repositories.Warehouses
{
    public class EfCoreWarehouseRepository : EfCoreRepository<WmsDbContext, Warehouse, Guid>, IWarehouseRepository
    {
        public EfCoreWarehouseRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Warehouse> FindByIdAsync(
            Guid id,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                //.AsNoTracking()
                .IncludeIf(includeDetails, o => o.WarehouseAreas)
                .FirstOrDefaultAsync(t => t.Id == id, GetCancellationToken(cancellationToken));
        }

        public async Task<Warehouse> FindByNameAsync(
            string warehouseName,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                //.AsNoTracking()
                .IncludeIf(includeDetails, o => o.WarehouseAreas)
                .FirstOrDefaultAsync(t => t.WarehouseName == warehouseName, GetCancellationToken(cancellationToken));
        }

        public async Task<Warehouse> FindByCodeAsync(
            string warehouseCode,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                //.AsNoTracking()
                .IncludeIf(includeDetails, o => o.WarehouseAreas)
                .FirstOrDefaultAsync(t => t.WarehouseCode == warehouseCode, GetCancellationToken(cancellationToken));
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync(
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .IncludeIf(includeDetails, o => o.WarehouseAreas)
                .OrderBy(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<QueryDataInPage<Warehouse>> GetPagedWarehousesAsync(
            string nameFilter = null,
            bool includeDetails = true,
            int maxResultCount = 10, 
            int skipCount = 0,  
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            var queryable = dbSet
                //.AsNoTracking()
                .WhereIf(!nameFilter.IsNullOrWhiteSpace(),
                    o => (o.WarehouseName.Contains(nameFilter)));

            return new QueryDataInPage<Warehouse>()
            {
                TotalCount = await queryable.CountAsync(),
                Items = await queryable
                    .IncludeIf(includeDetails, o => o.WarehouseAreas)
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
            };
        }

        public async Task<long> GetCountAsync(
            string nameFilter = null, 
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .AsNoTracking()
                .WhereIf(!nameFilter.IsNullOrWhiteSpace(), 
                    e => e.WarehouseName.Contains(nameFilter))
                .LongCountAsync(GetCancellationToken(cancellationToken));
        }


    }
}
