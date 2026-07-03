using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TuTa.Wms.Repositories.InBoundLists
{
    public class EfCoreChkResultListRepository : EfCoreRepository<WmsDbContext, ChkResultList, Guid>, IChkResultListRepository
    {
        public EfCoreChkResultListRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<ChkResultList> FindByBarcodeAndCheckTypeAsync(
            string barcode,
            EnumCheckType checkType,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .IncludeIf(includeDetails, o => o.ChkResultBoxes)
                .FirstOrDefaultAsync(o => o.Barcode == barcode && o.CheckData.CheckType == checkType, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<ChkResultList>> FindByChkNoAsync(
            string chkNo,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .IncludeIf(includeDetails, o => o.ChkResultBoxes)
                .Where(o => o.CheckData.CheckNo == chkNo)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<ChkResultList>> FindByBarcodeAsync(
            string barcode, 
            bool isTrack = true, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .IncludeIf(includeDetails, o => o.ChkResultBoxes)
                .Where(o => o.Barcode == barcode)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<ChkResultList>> FindByBarcodesAsync(
            List<string> barcodes,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => barcodes.Contains(o.Barcode))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
