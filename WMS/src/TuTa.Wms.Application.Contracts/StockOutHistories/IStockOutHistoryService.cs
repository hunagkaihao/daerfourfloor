using System.Threading.Tasks;
using TuTa.Wms.StockOutHistories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.StockOutHistories
{
    public interface IStockOutHistoryService : IApplicationService
    {
        Task<PagedResultDto<StockOutHistoryDto>> GetPagedStockOutHistoriesAsync(PagedStockOutHistoryQueryDto para);
    }
}
