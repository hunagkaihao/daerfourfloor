using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP收料工位应用服务
    /// </summary>
    public class ErpDeliveryStationAppService : ApplicationService, IErpDeliveryStationAppService
    {
        private readonly IErpDeliveryStationRepository _erpDeliveryStationRepository;
        private readonly ErpDeliveryStationManager _erpDeliveryStationManager;
        private readonly ILogger<ErpDeliveryStationAppService> _logger;

        public ErpDeliveryStationAppService(
            IErpDeliveryStationRepository erpDeliveryStationRepository,
            ErpDeliveryStationManager erpDeliveryStationManager,
            ILogger<ErpDeliveryStationAppService> logger)
        {
            _erpDeliveryStationRepository = erpDeliveryStationRepository;
            _erpDeliveryStationManager = erpDeliveryStationManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP收料工位数据
        /// </summary>
        [UnitOfWork]
        public async Task<ErpDeliveryStationResponseDto> ReceiveDeliveryStationAsync(ErpDeliveryStationRequestDto request)
        {
            try
            {
                // 验证请求数据
                if (request == null)
                    return new ErpDeliveryStationResponseDto { Succeed = false, Message = "请求数据不能为空" };

                // 验证数据
                _erpDeliveryStationManager.ValidateDeliveryStationData(
                    request.deliveryCode,
                    request.syncType);

                // 根据操作类型执行不同的逻辑
                switch (request.syncType?.ToLower())
                {
                    case "insert":
                        return await HandleInsertOperationAsync(request);
                    case "modify":
                        return await HandleModifyOperationAsync(request);
                    case "delete":
                        return await HandleDeleteOperationAsync(request);
                    default:
                        return new ErpDeliveryStationResponseDto 
                        { 
                            Succeed = false, 
                            Message = $"不支持的操作类型：{request.syncType}，支持的类型：insert、modify、delete" 
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP收料工位失败：{request?.deliveryCode}，操作类型：{request?.syncType}");
                return new ErpDeliveryStationResponseDto
                {
                    Succeed = false,
                    Message = $"收料工位接收失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 处理新增操作
        /// </summary>
        private async Task<ErpDeliveryStationResponseDto> HandleInsertOperationAsync(ErpDeliveryStationRequestDto request)
        {
            // 检查收料工位是否已存在
            var existingStation = await _erpDeliveryStationRepository.FindByDeliveryCodeAsync(request.deliveryCode);
            if (existingStation != null)
            {
                return new ErpDeliveryStationResponseDto
                {
                    Succeed = false,
                    Message = $"配送位置代号 {request.deliveryCode} 已存在，无法执行新增操作"
                };
            }

            // 创建新收料工位
            var deliveryStation = await _erpDeliveryStationManager.CreateOrUpdateDeliveryStationAsync(
                request.deliveryCode,
                request.syncTimeStamp,
                request.syncType);

            await _erpDeliveryStationRepository.InsertAsync(deliveryStation);

            _logger.LogInformation($"成功新增ERP收料工位：{request.deliveryCode}");
            return new ErpDeliveryStationResponseDto
            {
                Succeed = true,
                Message = "收料工位新增成功"
            };
        }

        /// <summary>
        /// 处理修改操作
        /// </summary>
        private async Task<ErpDeliveryStationResponseDto> HandleModifyOperationAsync(ErpDeliveryStationRequestDto request)
        {
            // 检查收料工位是否存在
            var existingStation = await _erpDeliveryStationRepository.FindByDeliveryCodeAsync(request.deliveryCode);
            if (existingStation == null)
            {
                return new ErpDeliveryStationResponseDto
                {
                    Succeed = false,
                    Message = $"配送位置代号 {request.deliveryCode} 不存在，无法执行修改操作"
                };
            }

            // 更新现有收料工位
            var deliveryStation = await _erpDeliveryStationManager.CreateOrUpdateDeliveryStationAsync(
                request.deliveryCode,
                request.syncTimeStamp,
                request.syncType);

            await _erpDeliveryStationRepository.UpdateAsync(deliveryStation);

            _logger.LogInformation($"成功修改ERP收料工位：{request.deliveryCode}");
            return new ErpDeliveryStationResponseDto
            {
                Succeed = true,
                Message = "收料工位修改成功"
            };
        }

        /// <summary>
        /// 处理删除操作
        /// </summary>
        private async Task<ErpDeliveryStationResponseDto> HandleDeleteOperationAsync(ErpDeliveryStationRequestDto request)
        {
            // 检查收料工位是否存在
            var existingStation = await _erpDeliveryStationRepository.FindByDeliveryCodeAsync(request.deliveryCode);
            if (existingStation == null)
            {
                return new ErpDeliveryStationResponseDto
                {
                    Succeed = false,
                    Message = $"配送位置代号 {request.deliveryCode} 不存在，无法执行删除操作"
                };
            }

            // 删除收料工位
            await _erpDeliveryStationRepository.DeleteAsync(existingStation);

            _logger.LogInformation($"成功删除ERP收料工位：{request.deliveryCode}");
            return new ErpDeliveryStationResponseDto
            {
                Succeed = true,
                Message = "收料工位删除成功"
            };
        }

        /// <summary>
        /// 根据ID获取收料工位
        /// </summary>
        public async Task<ErpDeliveryStationDto> GetAsync(Guid id)
        {
            var deliveryStation = await _erpDeliveryStationRepository.GetAsync(id);
            return ObjectMapper.Map<ErpDeliveryStation, ErpDeliveryStationDto>(deliveryStation);
        }

        /// <summary>
        /// 根据配送位置代号获取收料工位
        /// </summary>
        public async Task<ErpDeliveryStationDto> GetByDeliveryCodeAsync(string deliveryCode)
        {
            var deliveryStation = await _erpDeliveryStationRepository.FindByDeliveryCodeAsync(deliveryCode);
            if (deliveryStation == null)
                return null;

            return ObjectMapper.Map<ErpDeliveryStation, ErpDeliveryStationDto>(deliveryStation);
        }

        /// <summary>
        /// 获取收料工位列表
        /// </summary>
        public async Task<List<ErpDeliveryStationDto>> GetListAsync(
            string syncType = null,
            long? startTimeStamp = null,
            long? endTimeStamp = null)
        {
            List<ErpDeliveryStation> deliveryStations;

            if (!string.IsNullOrEmpty(syncType))
            {
                deliveryStations = await _erpDeliveryStationRepository.GetBySyncTypeAsync(syncType);
            }
            else if (startTimeStamp.HasValue && endTimeStamp.HasValue)
            {
                deliveryStations = await _erpDeliveryStationRepository.GetBySyncTimeStampRangeAsync(startTimeStamp.Value, endTimeStamp.Value);
            }
            else
            {
                deliveryStations = await _erpDeliveryStationRepository.GetListAsync();
            }

            return ObjectMapper.Map<List<ErpDeliveryStation>, List<ErpDeliveryStationDto>>(deliveryStations);
        }

        /// <summary>
        /// 删除收料工位
        /// </summary>
        [UnitOfWork]
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await _erpDeliveryStationRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除收料工位失败：{id}");
                return false;
            }
        }
    }
}
