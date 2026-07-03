using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Domain;

namespace TuTa.Wms.Boxes
{
    public interface IBoxRepository : IRepository<Box, Guid>
    {
        public Task<Box> FindByBoxIdAsync(
            Guid boxId,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        public Task<Box> FindByBoxCodeAsync(
            string boxCode,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default);

        public Task<Box> FindByBoxNameAsync(
            string boxName,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default);

        public Task<List<Box>> GetByCellsIdAsync(
            List<Guid> cellIds,
            bool isTrack = true,
            bool includeDetails = true, 
            CancellationToken cancellationToken = default);

        public Task<List<Box>> GetNoHaveByCellsIdAsync(
            List<Guid> cellIds,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        public Task<Box> FindByCellIdAsync(
            Guid cellId,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        public Task<List<Box>> GetAllAsync(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        public Task<List<Box>> GetNoHaveInAsync(
            int count,string type,List<string> cellCodes,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        public Task<QueryDataInPage<Box>> GetPagedBoxAsync(
            string boxCode, 
            string boxName, 
            Guid? cellId,
            int? warehouseAreaId,
            Guid? warehouseId,
            bool isTrack = true,
            bool includeDetails = true, 
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
    }
}
