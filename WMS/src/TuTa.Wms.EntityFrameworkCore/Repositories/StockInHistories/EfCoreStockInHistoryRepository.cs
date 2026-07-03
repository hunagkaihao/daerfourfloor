using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.StockInHistories;
using TuTa.Wms.StockInHistories.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.StockInHistories
{
    public class EfCoreStockInHistoryRepository : EfCoreRepository<WmsDbContext, StockInHistory, int>, IStockInHistoryRepository
    {
        public EfCoreStockInHistoryRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<QueryDataInPage<StockInHistory>> GetPagedStockInHistoriesAsync(
            string barcode,
            string materialCode, string materialNameTip, string materialSpecsTip,
            string stockInType,
            DateTime? stockInDateStart, DateTime? stockInDateEnd,
            string checkNoTip,
            bool isTrack = true, 
            int skipCount = 0, int maxResultCount = 10, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<StockInHistory> querable = dbSet.TrackIf(isTrack)
            .Where(o =>
                (barcode == null ? true : o.Barcode == barcode) &&
                (materialCode == null ? true : o.MaterialCode == materialCode) &&
                (materialNameTip == null ? true : o.MaterialName.Contains(materialNameTip)) &&
                (materialSpecsTip == null ? true : o.MaterialSpecs.Contains(materialSpecsTip)) &&
                (stockInType == null ? true : o.StockInType == stockInType) &&
                (stockInDateStart == null || stockInDateEnd == null ? true : o.InTime >= stockInDateStart && o.InTime <= stockInDateEnd));

            return new QueryDataInPage<StockInHistory>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }
    }
}
