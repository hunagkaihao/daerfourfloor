using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.BarcodeLists.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.BarcodeLists
{
    public interface IBarcodeListRepository:IRepository<BarcodeList,Guid>
    {
        Task<BarcodeList> FindByIdAsync(
            Guid id,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<BarcodeList> FindByBarcodeAsync(
            string barcode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<BarcodeList>> FindByBarcodesAsync(
            string barcode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<BarcodeList>> GetAllIsCheck(
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
    }
}
