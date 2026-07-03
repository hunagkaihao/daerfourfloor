using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.RecheckLists.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.RecheckLists
{
    public interface IRecheckListRepository : IRepository<RecheckList, Guid>
    {
        Task<RecheckList> FindByReCheckListCodeAsync(
            string reCheckListCode, 
            bool isTrack = true, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default);

        Task<List<RecheckList>> GetAllRecheckListsAsync(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
    }
}
