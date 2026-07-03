using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.Boxes;

[Route("wms/box")]
[ApiController]
public class BoxController : WmsController, IBoxService
{
    private readonly IBoxService _boxService;
    public BoxController(IBoxService boxService)
    {
        _boxService = boxService;
    }

    [HttpPost("boxAdd")]
    public async Task<ResponseDto> AddBoxAsync(BoxAddDto para)
    {
        return await _boxService.AddBoxAsync(para).ConfigureAwait(false);
    }

    [HttpPost("boxListAdd")]
    public async Task<ResponseDto> AddBoxListAsync(List<BoxAddDto> paras)
    {
        return await _boxService.AddBoxListAsync(paras).ConfigureAwait(false);
    }

    [HttpPost("boxDel")]
    public async Task<ResponseDto> DelBoxAsync(string boxCode)
    {
        return await _boxService.DelBoxAsync(boxCode).ConfigureAwait(false);
    }

    [HttpPost("boxAllDel")]
    public async Task<ResponseDto> DelAllBoxesAsync()
    {
        return await _boxService.DelAllBoxesAsync().ConfigureAwait(false);
    }

    [HttpPost("boxUpdate")]
    public async Task<ResponseDto> UpdateBoxAsync(Guid boxId, BoxUpdateDto para)
    {
        return await _boxService.UpdateBoxAsync(boxId, para).ConfigureAwait(false);
    }

    [HttpPost("pagedBoxesGet")]
    public async Task<PagedResultDto<BoxDto>> PagedBoxesGetAsync(PagedBoxesQueryDto para)
    {
        return await _boxService.PagedBoxesGetAsync(para).ConfigureAwait(false);
    }

    //[HttpPost("boxGoodsBind")]
    //public async Task<ResponseDto> BindGoodsAsync(Guid boxId, Guid goodsId)
    //{
    //    return await _boxService.BindGoodsAsync(boxId, goodsId).ConfigureAwait(false);
    //}
}