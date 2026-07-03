using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.PickLists;
using TuTa.Wms.PickLists.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.PickLists
{
    [Route("wms/pickList")]
    [ApiController]
    public class PickListController : WmsController, IPickListService
    {
        private readonly IPickListService _pickListService;

        private static readonly object _lock = new object();

        public PickListController(IPickListService pickListService)
        {
            _pickListService = pickListService;
        }

        [HttpPost("pickItemsGet")]
        [SwaggerOperation(Description = "查询领用项，PickType：1-生产领用，2-外协领用，14-超计划领用，15-无计划领用；OrderBy：1-按物料排序，2-按批次排序")]
        public async Task<List<PickItemDto>> GetUnFinishedPickItemsAsync(PickItemQueryDto para)
        {
            return await _pickListService.GetUnFinishedPickItemsAsync(para).ConfigureAwait(false);
        }

        [HttpPost("pagedPickItemsGet")]
        [SwaggerOperation(Summary = "分页查询领料单")]
        public async Task<PagedResultDto<PickItemDto>> GetPagedUnFinishedPickItemsAsync(PagedPickItemQueryDto para)
        {
            return await _pickListService.GetPagedUnFinishedPickItemsAsync(para).ConfigureAwait(false);
        }

        [HttpGet("pickItemsCnt")]
        [SwaggerOperation(Summary = "未完成的领用项的数量")]
        public async Task<int> GetUnFinishedPickItmCountAsync()
        {
            return await _pickListService.GetUnFinishedPickItmCountAsync().ConfigureAwait(false);
        }

        [HttpPost("pickStocksGet")]
        public async Task<List<PickStockDto>> AllocatePickStocksAsync(PickStockAllocateDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.AllocatePickStocksAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpPost("releasePickStock")]
        public async Task<ResponseDto> ReleasePickStockAsync(int pickItemId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.ReleasePickStockAsync(pickItemId).GetAwaiter().GetResult();
            }
        }

        [HttpPost("pickOut")]
        public async Task<ResponseDto> PickOutAsync(string pickListCode, string pickItemUniqueCode, PickOutDto para)
        {
            await Task.Delay(1);
            lock(_lock)
            {
                return _pickListService.PickOutAsync(pickListCode, pickItemUniqueCode, para).GetAwaiter().GetResult();
            }
        }


        [HttpPost("GetByBarcodeBoxCode")]
        [SwaggerOperation(Summary = "通过物料查询领料单")]
        public async Task<GetByBarcodeBoxDto> GetByBarcodeBoxCode(string barcode, string boxCode)
        {
            return await _pickListService.GetByBarcodeBoxCode(barcode, boxCode).ConfigureAwait(false);
        }

        [HttpPost("pickOutDown")]
        [SwaggerOperation(Summary = "料箱下架")]
        public async Task<ResponseDto> PickOutDownAsync(string startCellCode, string endCellCode, string pickListCode, string uniqueCode, string operatorName = null)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                string unique_name = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                {
                    var authHeader = authHeaderValues.FirstOrDefault();

                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadJwtToken(jwtToken);
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims));
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value));
                        unique_name = token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value;
                    }
                }
                return _pickListService.PickOutDownAsync(startCellCode, endCellCode, pickListCode, uniqueCode,unique_name).GetAwaiter().GetResult();
            }
        }

        [HttpPost("checkDown")]
        [SwaggerOperation(Summary = "检验下架")]
        public async Task<ResponseDto> CheckDownAsync(string startCellCode, string endCellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.CheckDownAsync(startCellCode, endCellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("noHaveDown")]
        [SwaggerOperation(Summary = "空箱下架")]
        public async Task<ResponseDto> NoHaveDownAsync(int count, string type,string area,string endArea)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.NoHaveDownAsync(count, type, area, endArea).GetAwaiter().GetResult();
            }
        }


        [HttpPost("pickOutByZZ")]
        [SwaggerOperation(Summary = "周转区出库")]
        public async Task<ResponseDto> PickOutByZZ(string barcode, string boxCode, decimal count, string pickListCode, string uniqueCode, string operatorName = null)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                string unique_name = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                {
                    var authHeader = authHeaderValues.FirstOrDefault();

                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadJwtToken(jwtToken);
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims));
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value));
                        unique_name = token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value;
                    }
                }
                return _pickListService.PickOutByZZ(barcode, boxCode, count, pickListCode, uniqueCode, unique_name).GetAwaiter().GetResult();
            }
        }


        [HttpPost("pickOutByBox")]
        [SwaggerOperation(Summary = "物料领用绑定")]
        public async Task<ResponseDto> PickOutByBox(string barcode, string boxCode, decimal count, string pickListCode, string uniqueCode, string nextBoxCode, string nextCellCode, string operatorName = null)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                string unique_name = null;
                if (Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                {
                    var authHeader = authHeaderValues.FirstOrDefault();

                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadJwtToken(jwtToken);
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims));
                        Console.WriteLine(JsonConvert.SerializeObject(token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value));
                        unique_name = token.Claims.Where(t => t.Type == "given_name").FirstOrDefault().Value;
                    }
                }
                return _pickListService.PickOutByBox(barcode, boxCode, count, pickListCode, uniqueCode, nextBoxCode, nextCellCode, unique_name).GetAwaiter().GetResult();
            }
        }

        private readonly object _locker = new object();
        [HttpPost("noPlanPickListCreate")]
        public async Task<ResponseDto> CreateNoPlanPickListAsync(NoPlanPickOutCreateDto para)
        {
            await Task.Delay(1);
            lock( _lock)
            {
                return _pickListService.CreateNoPlanPickListAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpPost("noPlanPickListDelete")]
        public async Task<ResponseDto> DeleteNoPlanPickListAsync(NoPlanPickListDelDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.DeleteNoPlanPickListAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpPost("noPlanPickListUpdate")]
        public async Task<ResponseDto> EditNoPlanPickListAsync(NoPlanPickListEditDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _pickListService.EditNoPlanPickListAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpGet("noPlanPickTypesGet")]
        public List<NoPlanPickTypeDto> GetAllNoPlanPickTypes()
        {
            return _pickListService.GetAllNoPlanPickTypes();
        }

        [HttpPost("noPlanPickListsGet")]
        public async Task<PagedResultDto<PickItemDto>> GetPagedNoPlanPickListAsync(PagedNoPlanPickItemsQueryDto para)
        {
            return await _pickListService.GetPagedNoPlanPickListAsync(para).ConfigureAwait(false);
        }

        [HttpGet("erptest")]
        public async Task<List<string>> GetErpDeptTypeDetail()
        {
            return await _pickListService.GetErpDeptTypeDetail().ConfigureAwait(false);
        }
    }
}
