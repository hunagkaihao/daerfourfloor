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
    /// ERP物料应用服务
    /// </summary>
    public class ErpMaterialAppService : ApplicationService, IErpMaterialAppService
    {
        private readonly IErpMaterialRepository _erpMaterialRepository;
        private readonly ErpMaterialManager _erpMaterialManager;
        private readonly ILogger<ErpMaterialAppService> _logger;

        public ErpMaterialAppService(
            IErpMaterialRepository erpMaterialRepository,
            ErpMaterialManager erpMaterialManager,
            ILogger<ErpMaterialAppService> logger)
        {
            _erpMaterialRepository = erpMaterialRepository;
            _erpMaterialManager = erpMaterialManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP物料数据
        /// </summary>
        [UnitOfWork]
        public async Task<ErpMaterialResponseDto> ReceiveMaterialAsync(ErpMaterialRequestDto request)
        {
            try
            {
                // 验证请求数据
                if (request == null)
                    return new ErpMaterialResponseDto { Succeed = false, Message = "请求数据不能为空" };

                // 验证数据
                _erpMaterialManager.ValidateMaterialData(
                    request.fGoodsCode,
                    request.fGoodsName,
                    request.fUnitCode,
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
                        return new ErpMaterialResponseDto 
                        { 
                            Succeed = false, 
                            Message = $"不支持的操作类型：{request.syncType}，支持的类型：insert、modify、delete" 
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP物料失败：{request?.fGoodsCode}，操作类型：{request?.syncType}");
                return new ErpMaterialResponseDto
                {
                    Succeed = false,
                    Message = $"物料接收失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 处理新增操作
        /// </summary>
        private async Task<ErpMaterialResponseDto> HandleInsertOperationAsync(ErpMaterialRequestDto request)
        {
            // 检查物料是否已存在
            var existingMaterial = await _erpMaterialRepository.FindByMaterialCodeAsync(request.fGoodsCode);
            if (existingMaterial != null)
            {
                return new ErpMaterialResponseDto
                {
                    Succeed = false,
                    Message = $"物料代号 {request.fGoodsCode} 已存在，无法执行新增操作"
                };
            }

            // 创建新物料
            var material = await _erpMaterialManager.CreateOrUpdateMaterialAsync(
                request.fGoodsCode,
                request.fGoodsName,
                request.fUnitCode,
                request.syncTimeStamp,
                request.syncType);

            await _erpMaterialRepository.InsertAsync(material);

            _logger.LogInformation($"成功新增ERP物料：{request.fGoodsCode}");
            return new ErpMaterialResponseDto
            {
                Succeed = true,
                Message = "物料新增成功"
            };
        }

        /// <summary>
        /// 处理修改操作
        /// </summary>
        private async Task<ErpMaterialResponseDto> HandleModifyOperationAsync(ErpMaterialRequestDto request)
        {
            // 检查物料是否存在
            var existingMaterial = await _erpMaterialRepository.FindByMaterialCodeAsync(request.fGoodsCode);
            if (existingMaterial == null)
            {
                return new ErpMaterialResponseDto
                {
                    Succeed = false,
                    Message = $"物料代号 {request.fGoodsCode} 不存在，无法执行修改操作"
                };
            }

            // 更新现有物料
            var material = await _erpMaterialManager.CreateOrUpdateMaterialAsync(
                request.fGoodsCode,
                request.fGoodsName,
                request.fUnitCode,
                request.syncTimeStamp,
                request.syncType);

            await _erpMaterialRepository.UpdateAsync(material);

            _logger.LogInformation($"成功修改ERP物料：{request.fGoodsCode}");
            return new ErpMaterialResponseDto
            {
                Succeed = true,
                Message = "物料修改成功"
            };
        }

        /// <summary>
        /// 处理删除操作
        /// </summary>
        private async Task<ErpMaterialResponseDto> HandleDeleteOperationAsync(ErpMaterialRequestDto request)
        {
            // 检查物料是否存在
            var existingMaterial = await _erpMaterialRepository.FindByMaterialCodeAsync(request.fGoodsCode);
            if (existingMaterial == null)
            {
                return new ErpMaterialResponseDto
                {
                    Succeed = false,
                    Message = $"物料代号 {request.fGoodsCode} 不存在，无法执行删除操作"
                };
            }

            // 删除物料
            await _erpMaterialRepository.DeleteAsync(existingMaterial);

            _logger.LogInformation($"成功删除ERP物料：{request.fGoodsCode}");
            return new ErpMaterialResponseDto
            {
                Succeed = true,
                Message = "物料删除成功"
            };
        }

        /// <summary>
        /// 根据ID获取物料
        /// </summary>
        public async Task<ErpMaterialDto> GetAsync(Guid id)
        {
            var material = await _erpMaterialRepository.GetAsync(id);
            return ObjectMapper.Map<ErpMaterial, ErpMaterialDto>(material);
        }

        /// <summary>
        /// 根据物料代号获取物料
        /// </summary>
        public async Task<ErpMaterialDto> GetByMaterialCodeAsync(string materialCode)
        {
            var material = await _erpMaterialRepository.FindByMaterialCodeAsync(materialCode);
            if (material == null)
                return null;

            return ObjectMapper.Map<ErpMaterial, ErpMaterialDto>(material);
        }

        /// <summary>
        /// 获取物料列表
        /// </summary>
        public async Task<List<ErpMaterialDto>> GetListAsync(
            string syncType = null,
            long? startTimeStamp = null,
            long? endTimeStamp = null)
        {
            List<ErpMaterial> materials;

            if (!string.IsNullOrEmpty(syncType))
            {
                materials = await _erpMaterialRepository.GetBySyncTypeAsync(syncType);
            }
            else if (startTimeStamp.HasValue && endTimeStamp.HasValue)
            {
                materials = await _erpMaterialRepository.GetBySyncTimeStampRangeAsync(startTimeStamp.Value, endTimeStamp.Value);
            }
            else
            {
                materials = await _erpMaterialRepository.GetListAsync();
            }

            return ObjectMapper.Map<List<ErpMaterial>, List<ErpMaterialDto>>(materials);
        }

        /// <summary>
        /// 删除物料
        /// </summary>
        [UnitOfWork]
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await _erpMaterialRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除物料失败：{id}");
                return false;
            }
        }
    }
}
