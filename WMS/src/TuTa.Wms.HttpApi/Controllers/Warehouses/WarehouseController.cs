using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.Warehouses
{
    [Route("wms/warehouse")]
    [ApiController]
    public class WarehouseController : WmsController, IWarehouseService
    {
        private readonly IWarehouseService _warehouseService;
        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost("warehouseAdd")]
        public async Task<ResponseDto> CreateWarehouseAsync(WarehouseAddDto para)
        {
            return await _warehouseService.CreateWarehouseAsync(para).ConfigureAwait(false);
        }

        [HttpPost("warehouseDel")]
        public async Task<ResponseDto> DeleteWarehouseAsync(Guid warehouseId)
        {
            return await _warehouseService.DeleteWarehouseAsync(warehouseId).ConfigureAwait(false);
        }

        [HttpPost("warehouseUpdate")]
        public async Task<ResponseDto> UpdateWarehouseAsync(Guid warehouseIdToUpdate, WarehouseUpdateDto para)
        {
            return await _warehouseService.UpdateWarehouseAsync(warehouseIdToUpdate, para).ConfigureAwait(false);
        }

        [HttpPost("pagedWarehouseGet")]
        public async Task<PagedResultDto<WarehouseDto>> GetPagedWarehouseListAsync(PagedWarehouseQueryDto para)
        {
            return await _warehouseService.GetPagedWarehouseListAsync(para).ConfigureAwait(false);
        }

        [HttpPost("warehouseAreaAdd")]
        public async Task<ResponseDto> AddWarehouseAreaAsync(Guid warehouseId, WarehouseAreaAddDto para)
        {
            return await _warehouseService.AddWarehouseAreaAsync(warehouseId, para).ConfigureAwait(false);
        }

        [HttpPost("warehouseAreaDel")]
        public async Task<ResponseDto> DelWarehouseAreaAsync(int areaIdToDel)
        {
            return await _warehouseService.DelWarehouseAreaAsync(areaIdToDel).ConfigureAwait(false);
        }

        [HttpPost("warehouseAreaUpdate")]
        public async Task<ResponseDto> UpdateWarehouseAreaAsync(int warehouseAreaIdToUpdate, WarehouseAreaUpdateDto para)
        {
            return await _warehouseService.UpdateWarehouseAreaAsync(warehouseAreaIdToUpdate, para).ConfigureAwait(false);
        }

        [HttpGet("warehouseAreasGet")]
        //[SwaggerOperation(summary: "查询周转区库区", Tags = new[] { "Warehouse" })]
        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseAsync(string warehouseName)
        {
            return await _warehouseService.GetAllAreasOfWarehouseAsync(warehouseName).ConfigureAwait(false);
        }

        [HttpGet("warehouseWorkShopGroupAreasGet")]
        //[SwaggerOperation(summary: "查询仓库库区", Tags = new[] { "Warehouse" })]
        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseWorkShopGroupAsync(string warehouseName)
        {
            return await _warehouseService.GetAllAreasOfWarehouseWorkShopGroupAsync(warehouseName).ConfigureAwait(false);
        }

        [HttpGet("warehouseGroupAreasGet")]
        //[SwaggerOperation(summary: "查询车间库区", Tags = new[] { "Warehouse" })]
        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseGroupAsync(string warehouseName)
        {
            return await _warehouseService.GetAllAreasOfWarehouseGroupAsync(warehouseName).ConfigureAwait(false);
        }

        [HttpGet("warehouseInOutAreasGet")]
        //[SwaggerOperation(summary: "查询周转区库区", Tags = new[] { "Warehouse" })]
        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseInOutAsync(string warehouseName)
        {
            return await _warehouseService.GetAllAreasOfWarehouseInOutAsync(warehouseName).ConfigureAwait(false);
        }

        [HttpGet("warehouseMoveAreasGet")]
        //[SwaggerOperation(summary: "查询周转区库区", Tags = new[] { "Warehouse" })]
        public async Task<List<WarehouseAreaDto>> GetAllAreasOfMoveAsync(string warehouseName)
        {
            return await _warehouseService.GetAllAreasOfMoveAsync(warehouseName).ConfigureAwait(false);
        }
    }
}
