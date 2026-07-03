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
    public interface IErpMoveRepository : IRepository<ErpMove>
    {
        public Task<List<ErpMove>> GetAllUnReceivedMovesAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);

    }
}
