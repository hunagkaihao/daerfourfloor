using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Dtos;
using TuTa.Wms.Skips.Dtos;

using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.Boxes;

[Route("wms/cell")]
[ApiController]
public class CellController : WmsController, ICellService
{
    private readonly ICellService _cellService;
    public CellController(ICellService cellService)
    {
        _cellService = cellService;
    }

    [HttpPost("cellAdd")]
    public async Task<ResponseDto> AddCellAsync(CellAddDto para)
    {
        return await _cellService.AddCellAsync(para).ConfigureAwait(false);
    }

    [HttpPost("cellsAdd")]
    public async Task<ResponseDto> AddCellsAsync(List<CellAddDto> paras)
    {
        return await _cellService.AddCellsAsync(paras).ConfigureAwait(false);
    }

    [HttpPost("cellsBindArea")]
    public async Task<ResponseDto> CellsBindToAreaAsync(CellsBindAreaDto para)
    {
        return await _cellService.CellsBindToAreaAsync(para).ConfigureAwait(false);
    }

    [HttpPost("cellsDisBindFromArea")]
    public async Task<ResponseDto> CellsDisBindToAreaAsync(CellsDisBindFromAreaDto para)
    {
        return await _cellService.CellsDisBindToAreaAsync(para).ConfigureAwait(false);
    }

    [HttpPost("cellDel")]
    public async Task<ResponseDto> DelCellAsync(Guid cellId)
    {
        return await _cellService.DelCellAsync(cellId).ConfigureAwait(false);
    }

    [HttpPost("pagedCellsGet")]
    public async Task<PagedResultDto<CellDto>> GetPagedCellsAsync(PagedCellsQueryDto para)
    {
        return await _cellService.GetPagedCellsAsync(para).ConfigureAwait(false);
    }

    [HttpPost("pagedCellsByArea")]
    public async Task<PagedResultDto<CellDto>> GetPagedCellsByAreaAsync(PagedCellsAreaDto para)
    {
        return await _cellService.GetPagedCellsByAreaAsync(para).ConfigureAwait(false);
    }

    [HttpPost("GetByStock")]
    [SwaggerOperation(summary: "根据条码容器分配库位", Tags = new[] { "Cell" })]
    public async Task<CellDto> GetCellByStock(string barcode,string boxCode)
    {
        return await _cellService.GetCellByStock(barcode, boxCode).ConfigureAwait(false);
    }

    [HttpPost("GetByWorkShop")]
    [SwaggerOperation(summary: "根据料车区域分配库位  ", Tags = new[] { "Cell" })]
    public async Task<CellDto> GetCellByWorkShop(string skipCode, int area)
    {
        return await _cellService.GetCellByWorkShop(skipCode, area).ConfigureAwait(false);
    }

    [HttpPost("GetBySkip")]
    [SwaggerOperation(summary: "根据料车区域分配库位  ", Tags = new[] { "Cell" })]
    public async Task<ResultGetBySkipDto> GetCellsBySkip(string skipCode)
    {
        return await _cellService.GetCellsBySkip(skipCode).ConfigureAwait(false);
    }

    [HttpPost("GetCellByPickOut")]
    [SwaggerOperation(summary: "根据物料领料单分配库位", Tags = new[] { "Cell" })]
    public async Task<CellDto> GetCellByPickOut(string barcode, string boxCode, string pickListCode, string uniqueCode)
    {
        return await _cellService.GetCellByPickOut(barcode, boxCode, pickListCode, uniqueCode).ConfigureAwait(false);
    }

    [HttpPost("GetCellByWall")]
    [SwaggerOperation(summary: "根据分配分拨墙库位", Tags = new[] { "Cell" })]
    public async Task<CellDto> GetCellByWall()
    {
        return await _cellService.GetCellByWall().ConfigureAwait(false);
    }

    [HttpPost("GetCellByCheck")]
    [SwaggerOperation(summary: "根据物料检验单分配库位", Tags = new[] { "Cell" })]
    public async Task<CellDto> GetCellByCheck(string barcode, string boxCode)
    {
        return await _cellService.GetCellByCheck(barcode, boxCode).ConfigureAwait(false);
    }

    [HttpGet("GetCtuArea")]
    [SwaggerOperation(summary: "获取CTU库区域", Tags = new[] { "Cell" })]
    public async Task<List<string>> GetCtuArea()
    {
        return await _cellService.GetCtuArea().ConfigureAwait(false);
    }

    [HttpGet("GetLaneCellStatusByCellCode")]
    [SwaggerOperation(summary: "通过库位编码查询同巷道库位的巷道位状态", Tags = new[] { "Cell" })]
    public async Task<List<CellLaneStatusDto>> GetLaneCellStatusByCellCodeAsync(string cellCode)
    {
        return await _cellService.GetLaneCellStatusByCellCodeAsync(cellCode).ConfigureAwait(false);
    }
}