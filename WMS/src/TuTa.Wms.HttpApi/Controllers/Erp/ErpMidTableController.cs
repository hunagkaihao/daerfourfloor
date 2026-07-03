using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Dtos;

namespace TuTa.Wms.Controllers.Erp;

[Route("wms/erp")]
[ApiController]
public class ErpMidTableController : WmsController, IErpMidTableService
{
    private readonly IErpMidTableService _erpMidTableService;
    public ErpMidTableController(IErpMidTableService erpMidTableService)
    {
        _erpMidTableService = erpMidTableService;
    }

    [HttpGet("pickerNamesGet")]
    public async Task<List<ErpPickManDto>> GetPickerNamesAsync(string nameTip)
    {
        return await _erpMidTableService.GetPickerNamesAsync(nameTip).ConfigureAwait(false);
    }

    //[HttpGet("goodsAftChkForBindBoxGet")]
    //public async Task<ErpGoodsAftChkDto> GetAftChkGoodsToGroupBoxByBarcodeAsync(string barcode)
    //{
    //    return await _erpMidTableService.GetAftChkGoodsToGroupBoxByBarcodeAsync(barcode).ConfigureAwait(false);
    //}
}
