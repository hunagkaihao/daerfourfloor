using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Dtos;
using TuTa.Wms.PickLists.Dtos;

using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.InBoundLists
{
    [Route("wms/chkResultList")]
    [ApiController]
    public class ChkResultListController : WmsController, IChkResultListService
    {
        private readonly IChkResultListService _chkResultListService;

        private static readonly object _locker = new object();

        private static readonly object _lock = new object();

        public ChkResultListController(IChkResultListService inboundListService)
        {
            _chkResultListService = inboundListService;
        }

        //[HttpPost("StocksCreateAndBindToCell")]
        //public async Task<ResponseDto> CreateStockAndBindToCellAsync(List<MaterialToInBoundDto> paras, string cellCode)
        //{
        //    await Task.Delay(1);
        //    lock (_lock)
        //    {
        //        return _inboundListService.CreateStockAndBindToCellAsync(paras, cellCode).GetAwaiter().GetResult();
        //    }
        //}

        [HttpGet("chkResultGet")]
        public async Task<ChkResultListDto> GetChkResultListByBarcodeAsync(string barcode)
        {
            return await _chkResultListService.GetChkResultListByBarcodeAsync(barcode).ConfigureAwait(false);
        }

        [HttpPost("checkDataCreateByOutHistory")]
        public async Task<ResponseDto> CreateChkResultListFromStockoutHistoryAsync(int stockoutHistoryId)
        {
            return await _chkResultListService.CreateChkResultListFromStockoutHistoryAsync(stockoutHistoryId).ConfigureAwait(false);
        }

        [HttpPost("GetPagedCheckInItems")]
        public async Task<PagedResultDto<ChkResultListDto>> GetPagedCheckInItemsAsync(PagedCheckItemQueryDto para)
        {
            return await _chkResultListService.GetPagedCheckInItemsAsync(para).ConfigureAwait(false);
        }

        [HttpPost("GetChkByBarcode")]
        public async Task<GetChkByBarcodeDto> GetCheckByBarcodeAsync(string barcode, string chkNo)
        {
            return await _chkResultListService.GetCheckByBarcodeAsync(barcode,chkNo).ConfigureAwait(false);
        }
    }
}

