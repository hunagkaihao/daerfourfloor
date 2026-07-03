using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.RecheckLists.Aggregates;
using TuTa.Wms.RecheckLists.Entities;

using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.RecheckLists
{
    public interface IRecheckItemRepository : IRepository<RecheckItem, int>
    {
        Task<RecheckItem> FindByCheckNoAsync(
            string checkNo,
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
