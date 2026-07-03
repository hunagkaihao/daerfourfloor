using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Domain;
using TuTa.Wms.StockOutHistories.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.StockOutHistories
{
    public interface IStockOutHistoryRepository : IRepository<StockOutHistory, int>
    {
        Task<QueryDataInPage<StockOutHistory>> GetPagedStockOutHistoriesAsync(
            string barcode,
            string materialCode, string materialNameTip, string materialSpecsTip,
            string stockOutType,
            DateTime? InOutDateStart, DateTime? InOutDateEnd,
            string checkNoTip, string pickBatchTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
    }
}
