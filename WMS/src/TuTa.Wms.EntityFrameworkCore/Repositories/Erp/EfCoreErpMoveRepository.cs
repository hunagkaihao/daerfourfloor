using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Wms.EntityFrameworkCore;
using System.Collections.Generic;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpMoveRepository : EfCoreRepository<ErpDbContext, ErpMove>, IErpMoveRepository
    {
        public EfCoreErpMoveRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {

        }

        public async Task<List<ErpMove>> GetAllUnReceivedMovesAsync(
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.IFJS != true)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
