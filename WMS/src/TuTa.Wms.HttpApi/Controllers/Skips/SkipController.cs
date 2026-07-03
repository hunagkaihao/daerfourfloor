using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace TuTa.Wms.Controllers.Skips
{
    [Route("wms/skip")]
    [ApiController]
    public class SkipController:WmsController,ISkipService
    {
        private readonly ISkipService _skipService;
        private static readonly object _lock = new object();

        public SkipController(ISkipService skipService)
        {
            _skipService = skipService;
        }

        [HttpPost("createSkip")]
        [SwaggerOperation(summary: "创建料车", Tags = new[] { "Skip" })]
        public async Task<ResponseDto>AddSkipAsync(SkipAddDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.AddSkipAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpPost("skipBindCell")]
        [SwaggerOperation(summary: "料车绑定库位", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> SkipBindCellAsync(string skipCode,string cellCode, string podDir, string isBind)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.SkipBindCellAsync(skipCode,cellCode,podDir,isBind).GetAwaiter().GetResult();
            }
        }

        [HttpPost("sendSkip")]
        [SwaggerOperation(summary: "发送料车", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> SendSkipAsync(string skipCode, string cellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.SendSkipAsync(skipCode, cellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("callSkip")]
        [SwaggerOperation(summary: "叫回料车", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> CallSkipAsync(string skipCode, int areaId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.CallSkipAsync(skipCode, areaId).GetAwaiter().GetResult();
            }
        }

        [HttpPost("getPagedSkips")]
        [SwaggerOperation(summary: "批量查询料车", Tags = new[] { "Skip" })]
        public async Task<PagedResultDto<SkipDto>> GetPagedSkips(PagedSkipDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.GetPagedSkips(para).GetAwaiter().GetResult();
            }
        }

        [HttpGet("getPagedSkipsOut")]
        [SwaggerOperation(summary: "查询下架料车", Tags = new[] { "Skip" })]
        public async Task<PagedResultDto<SkipOutDto>> GetPagedSkipsOut()
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.GetPagedSkipsOut().GetAwaiter().GetResult();
            }
        }

        [HttpPost("setNoHave")]
        [SwaggerOperation(summary: "设置为空料车", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> SetNoHaveStatus(string skipCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.SetNoHaveStatus(skipCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("setNoHaveWall")]
        [SwaggerOperation(summary: "设置为空分拨墙", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> ClearWallNoHaveBox()
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.ClearWallNoHaveBox().GetAwaiter().GetResult();
            }
        }

        [HttpPost("setReceipt")]
        [SwaggerOperation(summary: "设置为空料车", Tags = new[] { "Skip" })]
        public async Task<ResponseDto> SetReceipt(string skipCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _skipService.SetReceipt(skipCode).GetAwaiter().GetResult();
            }
        }
    }
}
