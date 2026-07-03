using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuTa.Wms.StockOutHistories;
using TuTa.Wms.StockOutHistories.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.StockOutHistories
{
    [Route("wms/stockOutHistory")]
    [ApiController]
    public class StockOutHistoryController : WmsController, IStockOutHistoryService
    {
        private readonly IStockOutHistoryService _stockOutHistoryService;

        //private static readonly object _lock = new object();

        public StockOutHistoryController(IStockOutHistoryService stockOutHistoryService)
        {
            _stockOutHistoryService = stockOutHistoryService;
        }

        [HttpPost("pagedStockOutHistoriesGet")]
        public async Task<PagedResultDto<StockOutHistoryDto>> GetPagedStockOutHistoriesAsync(PagedStockOutHistoryQueryDto para)
        {
            return await _stockOutHistoryService.GetPagedStockOutHistoriesAsync(para).ConfigureAwait(false);
        }
    }
}

