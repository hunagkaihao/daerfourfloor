using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuTa.Wms.StockInHistories;
using TuTa.Wms.StockInHistories.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.StockInHistories
{
    [Route("wms/stockInHistory")]
    [ApiController]
    public class StockInHistoryController : WmsController, IStockInHistoryService
    {
        private readonly IStockInHistoryService _stockHistoryService;

        //private static readonly object _lock = new object();

        public StockInHistoryController(IStockInHistoryService stockHistoryService)
        {
            _stockHistoryService = stockHistoryService;
        }

        [HttpPost("pagedStockInHistoriesGet")]
        public async Task<PagedResultDto<StockInHistoryDto>> GetPagedStockInHistoriesAsync(PagedStockInHistoryQueryDto para)
        {
            return await _stockHistoryService.GetPagedStockInHistoriesAsync(para).ConfigureAwait(false);
        }
    }
}

