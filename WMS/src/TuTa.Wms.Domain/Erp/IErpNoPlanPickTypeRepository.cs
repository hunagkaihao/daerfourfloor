using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpNoPlanPickTypeRepository : IRepository<ErpNoPlanPickType>
    {
        Task<List<ErpNoPlanPickType>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
