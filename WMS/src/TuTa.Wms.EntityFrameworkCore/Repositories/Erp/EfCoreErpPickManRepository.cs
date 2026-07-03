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
    public class EfCoreErpPickManRepository : EfCoreRepository<ErpDbContext, ErpPickMan>, IErpPickManRepository
    {
        public EfCoreErpPickManRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpPickMan>> GetPickManNamesAsync(
            string nameTip,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet.AsNoTracking()
                .Where(o => nameTip == null ? true : o.MAN_NAME.Contains(nameTip))
                .OrderBy(o => o.MAN_NAME)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
