using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpPickManRepository : IRepository<ErpPickMan>
    {
        Task<List<ErpPickMan>> GetPickManNamesAsync(string nameTip, CancellationToken cancellationToken = default);
    }
}
