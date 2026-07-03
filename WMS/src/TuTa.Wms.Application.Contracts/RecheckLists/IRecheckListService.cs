using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.RecheckLists.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.RecheckLists
{
    public interface IRecheckListService : IApplicationService
    {
        Task<int> GetUnFinishedRecheckItemsCountAsync();

        Task<List<RecheckItemDto>> GetUnFinishedRecheckItemsAsync(RecheckItemQueryDto para);

        Task<List<RecheckStockDto>> GetRecheckStocksAsync(string recheckListCode, string barcode);

        Task<ResponseDto> RecheckStockPickOutAsync(RecheckPickOutDto para);
    }
}
