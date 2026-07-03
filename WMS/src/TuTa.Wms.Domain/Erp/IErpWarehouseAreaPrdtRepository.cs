using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpWarehouseAreaPrdtRepository : IRepository<ErpWarehouseAreaPrdt>
    {
        Task<string> GetAreaByPrdtName(
            string name,string dept,
            bool isTrack = true,
            CancellationToken cancellationToken = default);
    }
}
