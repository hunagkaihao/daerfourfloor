using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Application.Contracts.Shared;
using Wms.LogTool;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Dtos;
using TuTa.Wms.Warehouses.Entities;

namespace TuTa.Wms.Warehouses
{
    public class WarehouseService : WmsAppService, IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly WarehouseManager _warehouseManager;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(
            IWarehouseRepository warehouseRepository,
            WarehouseManager warehouseManager,
            ILogger<WarehouseService> logger)
        {
            _warehouseRepository = warehouseRepository;
            _warehouseManager = warehouseManager;
            _logger = logger;
        }

        public async Task<ResponseDto> CreateWarehouseAsync(WarehouseAddDto para)
        {
            try
            {
                if (!Enum.IsDefined(typeof(WarehouseType), para.WarehouseType))
                    throw new Exception($"仓库类型{para.WarehouseType}无法识别");

                if (!Enum.TryParse<WarehouseType>(para.WarehouseType, out var type))
                    throw new Exception($"仓库类型{para.WarehouseType}无法识别");

                Warehouse house = await _warehouseManager.CreateWarehouseAsync(
                    para.WarehouseCode, para.WarehouseName,
                    type, para.WarehouseRemark, null, null);

                await _warehouseRepository.InsertAsync(house).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "创建仓库成功" };
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DeleteWarehouseAsync(Guid warehouseId)
        {
            try
            {
                Warehouse houseExist = await _warehouseRepository.FindByIdAsync(warehouseId).ConfigureAwait(false);
                if (houseExist == null)
                    return new ResponseDto() { success = true, message = $"Id为{warehouseId}的仓库不存在，默认删除成功!" };

                await _warehouseRepository.DeleteAsync(houseExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "删除成功!" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> UpdateWarehouseAsync(Guid warehouseIdToUpdate, WarehouseUpdateDto para)
        {
            try
            {
                if (!Enum.IsDefined(typeof(WarehouseType), para.WarehouseTypeNew))
                    throw new Exception($"仓库类型{para.WarehouseTypeNew}无法识别");

                if (!Enum.TryParse<WarehouseType>(para.WarehouseTypeNew, out var type))
                    throw new Exception($"仓库类型{para.WarehouseTypeNew}无法识别");

                Warehouse houseExist = await _warehouseRepository.FindByIdAsync(warehouseIdToUpdate).ConfigureAwait(false);
                if (houseExist == null)
                    return new ResponseDto() { success = false, message = $"Id为{warehouseIdToUpdate}的仓库不存在，更新失败!" };

                await _warehouseManager.UpdateWarehouseAsync(houseExist, para.WarehouseCodeNew, para.WarehouseNameNew,
                    type, para.WarehouseRemarkNew, null, null);

                await _warehouseRepository.UpdateAsync(houseExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "更新成功!" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<WarehouseDto>> GetPagedWarehouseListAsync(PagedWarehouseQueryDto para)
        {
            try
            {
                var result = await _warehouseRepository.GetPagedWarehousesAsync(
                    para.NameFilter, 
                    true,
                    para.MaxResultCount, 
                    para.SkipCount)
                    .ConfigureAwait(false);

                var resultDto = new PagedResultDto<WarehouseDto>
                {
                    TotalCount = result.TotalCount,
                };

                List<WarehouseDto> items = new List<WarehouseDto>();
                foreach(var item in result.Items)
                {
                    WarehouseDto dto = new WarehouseDto()
                    {
                        Id = item.Id,
                        WarehouseCode = item.WarehouseCode,
                        WarehouseName = item.WarehouseName,
                        WarehouseFlag = item.WarehouseFlag,
                        WarehouseType = item.WarehouseType.ToString(),
                        WarehouseRemark = item.WarehouseRemark,
                        WarehouseOrder = item.WarehouseOrder
                    };
                    items.Add(dto);
                }
                resultDto.Items = items;

                return resultDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> AddWarehouseAreaAsync(Guid warehouseId, WarehouseAreaAddDto para)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByIdAsync(warehouseId).ConfigureAwait(false);
                if (house == null)
                    return new ResponseDto() { success = false, message = $"Id为{warehouseId}的仓库不存在，添加库区失败" };

                house.AddArea(para.WarehouseAreaCode, para.WarehouseAreaName, para.WarehouseAreaRemark,
                    para.WarehouseAreaFlag, para.WarehouseAreaOrder, para.WarehouseAreaGroup);

                await _warehouseRepository.UpdateAsync(house).ConfigureAwait(false);

                return new ResponseDto { success = true, message = "添加库区成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DelWarehouseAreaAsync(int warehouseAreaId)
        {
            try
            {
                var houses = await _warehouseRepository.GetAllWarehousesAsync().ConfigureAwait(false);
                if (houses == null || houses.Count == 0)
                    return new ResponseDto() { success = true, message = "当前没有定义仓库，也没有定义库区，默认删除成功" };

                foreach(var house in houses)
                {
                    var area = house.GetAreaByAreaId(warehouseAreaId);
                    if (area == null) 
                        continue;
                    else
                    {
                        house.RemoveArea(warehouseAreaId);
                        await _warehouseRepository.UpdateAsync(house);
                        return new ResponseDto { success = true, message = "删除库区成功" };
                    }
                }

                return new ResponseDto { success = true, message = $"不存在Id为{warehouseAreaId}的库区，默认删除库区成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> UpdateWarehouseAreaAsync(int warehouseAreaIdToUpdate, WarehouseAreaUpdateDto para)
        {
            try
            {
                List<Warehouse> houses = await _warehouseRepository.GetAllWarehousesAsync().ConfigureAwait(false);
                if (houses == null || houses.Count == 0)
                    return new ResponseDto() { success = false, message = $"当前没有定义仓库，也没有定义库区，更新库区失败" };

                Warehouse houseToModify = null;
                foreach(var house in houses)
                {
                    WarehouseArea areaToModify = house.GetAreaByAreaId(warehouseAreaIdToUpdate);
                    if (areaToModify != null)
                    {
                        houseToModify = house;
                        break;
                    }
                }

                if (houseToModify == null)
                    return new ResponseDto() { success = false, message = $"未找到Id为{warehouseAreaIdToUpdate}的库区，更新库区失败" };

                houseToModify.ModifyArea(
                    warehouseAreaIdToUpdate, para.WarehouseAreaCodeNew,
                    para.WarehouseAreaNameNew, para.WarehouseAreaRemarkNew, 
                    para.WarehouseAreaFlagNew, para.WarehouseAreaOrderNew, 
                    para.WarehouseAreaGroupNew);

                await _warehouseRepository.UpdateAsync(houseToModify).ConfigureAwait(false);

                return new ResponseDto { success = true, message = "更新库区成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseAsync(string warehouseName)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByIdAsync(Guid.Parse(warehouseName)).ConfigureAwait(false);
                if (house == null)
                    return new List<WarehouseAreaDto>();

                return ObjectMapper.Map<List<WarehouseArea>, List<WarehouseAreaDto>>(house.WarehouseAreas);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseWorkShopGroupAsync(string warehouseName)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByNameAsync(warehouseName).ConfigureAwait(false);
                if (house == null)
                    return new List<WarehouseAreaDto>();

                return ObjectMapper.Map<List<WarehouseArea>, List<WarehouseAreaDto>>(house.GetAreasForSkip());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseGroupAsync(string warehouseName)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByNameAsync(warehouseName).ConfigureAwait(false);
                if (house == null)
                    return new List<WarehouseAreaDto>();

                return ObjectMapper.Map<List<WarehouseArea>, List<WarehouseAreaDto>>(house.GetAreasForWarehouse());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<WarehouseAreaDto>> GetAllAreasOfWarehouseInOutAsync(string warehouseName)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByNameAsync(warehouseName).ConfigureAwait(false);
                if (house == null)
                    return new List<WarehouseAreaDto>();

                return ObjectMapper.Map<List<WarehouseArea>, List<WarehouseAreaDto>>(house.GetAreas());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<WarehouseAreaDto>> GetAllAreasOfMoveAsync(string warehouseName)
        {
            try
            {
                Warehouse house = await _warehouseRepository.FindByNameAsync(warehouseName).ConfigureAwait(false);
                if (house == null)
                    return new List<WarehouseAreaDto>();

                return ObjectMapper.Map<List<WarehouseArea>, List<WarehouseAreaDto>>(house.GetAreasForMove());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
