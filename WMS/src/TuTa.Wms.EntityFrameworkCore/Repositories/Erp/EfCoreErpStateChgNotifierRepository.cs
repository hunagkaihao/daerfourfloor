using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpStateChgNotifierRepository : EfCoreRepository<ErpDbContext, ErpStateChgNotifier>, IErpStateChgNotifierRepository
    {
        public EfCoreErpStateChgNotifierRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpStateChgNotifier>> GetAllUnReceivedNotifiersAsync(CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .Where(o => o.IFJS != true)
                .OrderBy(o => o.CKZTCHANG_DATE)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
