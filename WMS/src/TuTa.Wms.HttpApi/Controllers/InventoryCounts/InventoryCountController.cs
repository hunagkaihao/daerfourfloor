using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.InventoryCounts;

namespace TuTa.Wms.Controllers.InventoryCounts;

/// <summary>PDA盘点接口，保存实盘结果及按保存日期导出Excel。</summary>
[ApiController]
[Route("wms/inventory-count")]
public class InventoryCountController : WmsController
{
    /// <summary>盘点应用服务。</summary>
    private readonly IInventoryCountService _service;

    /// <summary>注入盘点应用服务。</summary>
    public InventoryCountController(IInventoryCountService service) => _service = service;

    /// <summary>保存一笔盘点明细，服务端解析扫码内容并计算总数。</summary>
    [HttpPost]
    public Task<ResponseDto> CreateAsync([FromBody] InventoryCountCreateDto input) => _service.CreateAsync(input);

    /// <summary>导出指定保存日期的全部盘点明细，日期格式yyyy-MM-dd。</summary>
    [HttpGet("export")]
    public async Task<FileContentResult> ExportAsync([FromQuery] string date)
    {
        var bytes = await _service.ExportAsync(date);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"盘点数据_{date}.xlsx");
    }
}
