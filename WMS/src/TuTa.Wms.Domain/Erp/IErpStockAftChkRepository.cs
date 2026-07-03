using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using TuTa.Wms.Erp.Entities;
using System.Collections.Generic;

namespace TuTa.Wms.Erp
{
    public interface IErpStockAftChkRepository : IRepository<ErpStockAftChk>
    {
        public Task<List<ErpStockAftChk>> GetAllUnReceivedStocksAsync(
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        public Task<ErpStockAftChk> FindByBarcodeAsync(
            string barcode, 
            bool isTrack = true,
            CancellationToken cancellationToken = default);

    }
}
