using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Dtos;

namespace TuTa.Wms.Controllers.RecheckLists
{
    [Route("wms/recheckList")]
    [ApiController]
    public class RecheckListController : WmsController, IRecheckListService
    {
        private readonly IRecheckListService _recheckListService;

        private static readonly object _lock = new object();


        public RecheckListController(IRecheckListService recheckListService)
        {
            _recheckListService = recheckListService;
        }

        [HttpGet("recheckItemsCount")]
        public async Task<int> GetUnFinishedRecheckItemsCountAsync()
        {
            return await _recheckListService.GetUnFinishedRecheckItemsCountAsync().ConfigureAwait(false);
        }

        [HttpPost("recheckItemsGet")]
        public async Task<List<RecheckItemDto>> GetUnFinishedRecheckItemsAsync(RecheckItemQueryDto para)
        {
            return await _recheckListService.GetUnFinishedRecheckItemsAsync(para).ConfigureAwait(false);
        }

        [HttpGet("recheckStocksGet")]
        public async Task<List<RecheckStockDto>> GetRecheckStocksAsync(string recheckListCode, string barcode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _recheckListService.GetRecheckStocksAsync(recheckListCode, barcode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("recheckStockPickOut")]
        public async Task<ResponseDto> RecheckStockPickOutAsync(RecheckPickOutDto para)
        {
            await Task.Delay(1);
            lock(_lock)
            {
                return _recheckListService.RecheckStockPickOutAsync(para).GetAwaiter().GetResult();
            }            
        }
    }
}

