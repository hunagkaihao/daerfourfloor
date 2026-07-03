using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.EntityFrameworkCore.Repositories;
using TuTa.Wms.StockOutHistories;
using TuTa.Wms.StockOutHistories.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.StockOutHistories
{
    public class EfCoreStockOutHistoryRepository : EfCoreRepository<WmsDbContext, StockOutHistory, int>, IStockOutHistoryRepository
    {
        public EfCoreStockOutHistoryRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<QueryDataInPage<StockOutHistory>> GetPagedStockOutHistoriesAsync(
            string barcode, 
            string materialCode, string materialNameTip, string materialSpecsTip, 
            string stockOutType, 
            DateTime? outDateStart, DateTime? outDateEnd,
            string checkNoTip, string pickBatchTip,
            bool isTrack = true, 
            int skipCount = 0, 
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<StockOutHistory> query = isTrack ? dbSet : dbSet.AsNoTracking();
            
            query = query
                .Where(o =>
                    (string.IsNullOrWhiteSpace(barcode) ? true : o.Barcode.Contains(barcode)) &&
                    (string.IsNullOrWhiteSpace(materialCode) ? true : o.MaterialCode.Contains(materialCode)) &&
                    (string.IsNullOrWhiteSpace(materialNameTip) ? true : o.MaterialName.Contains(materialNameTip)) &&
                    (string.IsNullOrWhiteSpace(materialSpecsTip) ? true : o.MaterialSpecs.Contains(materialSpecsTip)) &&
                    (string.IsNullOrWhiteSpace(stockOutType) ? true : o.StockOutType == stockOutType) &&
                    (outDateStart == null || outDateEnd == null ? true : o.OutTime >= outDateStart && o.OutTime <= outDateEnd));

            return new QueryDataInPage<StockOutHistory>()
            {
                TotalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false),
                Items = await query
                    .OrderByDescending(o => o.OutTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }
    }
}