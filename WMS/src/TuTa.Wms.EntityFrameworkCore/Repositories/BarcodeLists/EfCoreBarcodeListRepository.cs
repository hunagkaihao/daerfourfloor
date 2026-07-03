using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.EntityFrameworkCore;

using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.BarcodeLists
{
    public class EfCoreBarcodeListRepository : EfCoreRepository<WmsDbContext, BarcodeList, Guid>, IBarcodeListRepository
    {
        public EfCoreBarcodeListRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {

        }
        public async Task<BarcodeList> FindByIdAsync(
            Guid id,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<BarcodeList> FindByBarcodeAsync(
            string barcode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.Barcode == barcode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<BarcodeList>> FindByBarcodesAsync(
            string barcode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.Barcode == barcode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<List<BarcodeList>> GetAllIsCheck(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.isCheckOut=="1")
                .OrderBy(o=>o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
