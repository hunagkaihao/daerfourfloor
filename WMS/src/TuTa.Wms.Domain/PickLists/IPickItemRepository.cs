using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists.Entities;

using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.PickLists
{
    public interface IPickItemRepository : IRepository<PickItem, int>
    {
        Task<List<PickItem>> GetByMaterial(
            string materialCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<PickItem> GetByUnique(
            string unique,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
    }
}
 