using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Dtos;

namespace TuTa.Wms.Controllers.BarcodeLists
{
    [Route("wms/barcode")]
    [ApiController]
    public class BarcodeListController:WmsController,IBarcodeListService
    {
        private readonly IBarcodeListService _barcodeListService;

        public BarcodeListController(IBarcodeListService barcodeListService)
        {
            _barcodeListService = barcodeListService;
        }

        [HttpGet("barcodeGet")]
        public async Task<BarcodeDto> GetBarcodeAsync(string barcode)
        {
            return await _barcodeListService.GetBarcodeAsync(barcode);
        }


        [HttpGet("barcodeGetPrint")]
        public async Task<BarcodeDto> GetBarcodePrintAsync(string barcode)
        {
            return await _barcodeListService.GetBarcodePrintAsync(barcode);
        }
    }
}
