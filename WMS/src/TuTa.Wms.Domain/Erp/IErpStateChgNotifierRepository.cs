using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpStateChgNotifierRepository : IRepository<ErpStateChgNotifier>
    {
        Task<List<ErpStateChgNotifier>> GetAllUnReceivedNotifiersAsync(CancellationToken cancellationToken = default);
    }
}
