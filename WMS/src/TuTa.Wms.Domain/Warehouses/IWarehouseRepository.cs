using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.Warehouses.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Warehouses
{
    public interface IWarehouseRepository : IRepository<Warehouse, Guid>
    {
        Task<Warehouse> FindByIdAsync(
            Guid id,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<Warehouse> FindByNameAsync(
            string warehouseName,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<Warehouse> FindByCodeAsync(
            string warehouseCode,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Warehouse>> GetAllWarehousesAsync(
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<QueryDataInPage<Warehouse>> GetPagedWarehousesAsync(
            string nameFilter = null,
            bool includeDetails = true,
            int maxResultCount = 10,
            int skipCount = 0,
            CancellationToken cancellationToken = default);

        Task<long> GetCountAsync(
            string nameFilter = null,
            CancellationToken cancellationToken = default);
    }
}
