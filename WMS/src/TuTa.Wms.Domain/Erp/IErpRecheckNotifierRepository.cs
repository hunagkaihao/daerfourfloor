using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpRecheckNotifierRepository : IRepository<ErpRecheckNotifier>
    {
        Task<List<ErpRecheckNotifier>> GetAllUnReceivedRecheckNotifiersAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        Task<List<ErpRecheckNotifier>> GetRecheckNotifiersWithNotifierCodeAsync(
            string notifierCode,
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
