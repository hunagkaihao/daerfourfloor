using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeChecks.Aggregates;
using TuTa.Wms.BarcodeLists.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.BarcodeLists
{
    public interface IBarcodeCheckRepository : IRepository<BarcodeCheck,Guid>
    {
        //Task<BarcodeList> FindByBarcodeAsync(
        //    string barcode,
        //    bool isTrack = true,
        //    bool includeDetails = true,
        //    CancellationToken cancellationToken = default);

        Task<List<BarcodeCheck>> GetByBoxAsync(
            Guid boxid,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<BarcodeCheck> GetByBarcodeAsync(
            Guid barcodeid,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
    }
}
