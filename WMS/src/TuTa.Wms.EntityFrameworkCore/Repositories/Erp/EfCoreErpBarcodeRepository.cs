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
    public class EfCoreErpBarcodeRepository : EfCoreRepository<ErpDbContext, ErpBarcode>, IErpBarcodeRepository
    {
        public EfCoreErpBarcodeRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        //public async Task<ErpStockAftChk> FindByBarcodeAsync(
        //    string barcode, 
        //    bool isTrack = true,
        //    CancellationToken cancellationToken = default)
        //{
        //    var dbSet = await GetDbSetAsync();
        //    return await dbSet
        //        .TrackIf(isTrack)
        //        .Where(o => o.DHTZD_TXM == barcode && o.IFDELETE != true)
        //        .FirstOrDefaultAsync(cancellationToken)
        //        .ConfigureAwait(false);
        //}

        public async Task<List<ErpBarcode>> GetAllUnReceivedStocksAsync(
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.IFJS != true)
                .OrderBy(o => o.DHTZD_TXM)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
