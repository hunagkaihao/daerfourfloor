using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpDeptTypeRepository : IRepository<ErpDeptType>
    {
        Task<string> GetIdByName(string name, CancellationToken cancellationToken = default);
    }
}
