using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Erp
{
    public class EfCoreErpPickOrderRepository : EfCoreRepository<ErpDbContext, ErpPickOrder>, IErpPickOrderRepository
    {
        public EfCoreErpPickOrderRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ErpPickOrder>> GetAllUnReceivedPickOrdersAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.IFJS != true)
                .OrderBy(o => o.CHKTZD_ID)
                .ThenBy(o => o.PRDT_ID)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<ErpPickOrder>> GetPickOrdersWithListCodeAsync(
            string pickListCode, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.CHKTZD_ID == pickListCode)
                .OrderBy(o => o.PRDT_ID)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
