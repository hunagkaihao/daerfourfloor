using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpDeptTypeDetailRepository : IRepository<ErpDeptTypeDetail>
    {
        Task<ErpDeptTypeDetail> FindByDeptMaterial(string dept,string material,CancellationToken cancellationToken = default);
    }
}
