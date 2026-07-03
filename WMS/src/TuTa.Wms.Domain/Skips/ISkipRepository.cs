using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.Domain;
using TuTa.Wms.Skips.Aggregates;

using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Skips
{
    public interface ISkipRepository:IRepository<Skip,Guid>
    {
        Task<Skip> FindBySkipCodeAsync(
            string skipCode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<Skip> FindByCellIdAsync(
            Guid? cellId,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Skip>> FindInZhouZhuanAsync(
            List<Guid> Ids,int type,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<QueryDataInPage<Skip>> GetPagedSkipsAsync(
            int warehouseAreaId,
            SkipStatus status,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
    }
}
