using System;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.StockInHistories.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.StockInHistories
{
    public interface IStockInHistoryRepository : IRepository<StockInHistory, int>
    {
        Task<QueryDataInPage<StockInHistory>> GetPagedStockInHistoriesAsync(
            string barcode,
            string materialCode, string materialNameTip, string materialSpecsTip,
            string stockInType,
            DateTime? stockInDateStart, DateTime? stockInDateEnd,
            string checkNoTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
    }
}
