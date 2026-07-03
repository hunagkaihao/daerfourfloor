using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Warehouses.Dtos;

namespace TuTa.Wms.Warehouses
{
    public interface IWarehouseService : IApplicationService
    {

        /// <summary>
        /// 新增仓库
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateWarehouseAsync(WarehouseAddDto para);

        /// <summary>
        /// 删除仓库
        /// </summary>
        Task<ResponseDto> DeleteWarehouseAsync(Guid warehouseId);

        /// <summary>
        /// 更新仓库
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<ResponseDto> UpdateWarehouseAsync(Guid warehouseIdToUpdate, WarehouseUpdateDto para);

        /// <summary>
        /// 分页查询仓库
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<WarehouseDto>> GetPagedWarehouseListAsync(PagedWarehouseQueryDto para);

        /// <summary>
        /// 添加库区
        /// </summary>
        /// <returns></returns>
        Task<ResponseDto> AddWarehouseAreaAsync(Guid warehouseId, WarehouseAreaAddDto para);

        /// <summary>
        /// 删除库区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <param name="areaCodeToDel"></param>
        /// <returns></returns>
        Task<ResponseDto> DelWarehouseAreaAsync(int warehouseAreaId);

        /// <summary>
        /// 更改库区
        /// </summary>
        /// <param name="warehouseAreaIdToUpdate"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> UpdateWarehouseAreaAsync(int warehouseAreaIdToUpdate, WarehouseAreaUpdateDto para);

        /// <summary>
        /// 获取仓库的所有分区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseAsync(string warehouseName);

        /// <summary>
        /// 获取所有车间分区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseWorkShopGroupAsync(string warehouseName);

        /// <summary>
        /// 获取所有仓库分区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseGroupAsync(string warehouseName);

        /// <summary>
        /// 获取仓库的出入库分区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseInOutAsync(string warehouseName);

        /// <summary>
        /// 获取调拨的出入库分区
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>
        Task<List<WarehouseAreaDto>> GetAllAreasOfMoveAsync(string warehouseName);

    }
}
