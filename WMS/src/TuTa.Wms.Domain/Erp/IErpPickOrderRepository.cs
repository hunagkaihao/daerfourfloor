using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpPickOrderRepository : IRepository<ErpPickOrder>
    {
        Task<List<ErpPickOrder>> GetAllUnReceivedPickOrdersAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        Task<List<ErpPickOrder>> GetPickOrdersWithListCodeAsync(
            string pickListCode,
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
