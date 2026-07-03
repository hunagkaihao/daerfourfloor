using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeChecks.Aggregates;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.EntityFrameworkCore;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.BarcodeLists
{
    public class EfCoreBarcodeCheckRepository : EfCoreRepository<WmsDbContext, BarcodeCheck, Guid>, IBarcodeCheckRepository
    {
        public EfCoreBarcodeCheckRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {

        }
        //public async Task<BarcodeList> FindByBarcodeAsync(
        //    string barcode,
        //    bool isTrack = true,
        //    bool includeDetails = true,
        //    CancellationToken cancellationToken = default)
        //{
        //    var dbSet = await GetDbSetAsync().ConfigureAwait(false);
        //    return await dbSet
        //        .TrackIf(isTrack)
        //        .FirstOrDefaultAsync(o => o.Barcode == barcode, cancellationToken)
        //        .ConfigureAwait(false);
        //}

        public async Task<List<BarcodeCheck>> GetByBoxAsync(
            Guid boxid,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.BoxId == boxid)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<BarcodeCheck> GetByBarcodeAsync(
            Guid barcodeid,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.BarcodeId == barcodeid && o.BoxId == null);
        }
    }
}
