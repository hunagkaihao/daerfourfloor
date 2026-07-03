using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.ChkResultLists.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.ChkResultLists
{
    public interface IChkResultListRepository : IRepository<ChkResultList, Guid>
    {
        Task<ChkResultList> FindByBarcodeAndCheckTypeAsync(
            string barcode,
            EnumCheckType checkType,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<ChkResultList>> FindByChkNoAsync(
            string chkNo,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<ChkResultList>> FindByBarcodeAsync(
            string barcode,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<ChkResultList>> FindByBarcodesAsync(
            List<string> barcodes,
            bool isTrack = true,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
    }
}
