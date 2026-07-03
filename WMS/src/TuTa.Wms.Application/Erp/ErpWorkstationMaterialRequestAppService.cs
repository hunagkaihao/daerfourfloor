using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP工位叫料任务应用服务
    /// </summary>
    public class ErpWorkstationMaterialRequestAppService : ApplicationService, IErpWorkstationMaterialRequestAppService
    {
        private readonly IErpWorkstationMaterialRequestRepository _erpWorkstationMaterialRequestRepository;
        private readonly ErpWorkstationMaterialRequestManager _erpWorkstationMaterialRequestManager;
        private readonly ILogger<ErpWorkstationMaterialRequestAppService> _logger;

        public ErpWorkstationMaterialRequestAppService(
            IErpWorkstationMaterialRequestRepository erpWorkstationMaterialRequestRepository,
            ErpWorkstationMaterialRequestManager erpWorkstationMaterialRequestManager,
            ILogger<ErpWorkstationMaterialRequestAppService> logger)
        {
            _erpWorkstationMaterialRequestRepository = erpWorkstationMaterialRequestRepository;
            _erpWorkstationMaterialRequestManager = erpWorkstationMaterialRequestManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP工位叫料任务
        /// </summary>
        [UnitOfWork]
        public async Task<ErpWorkstationMaterialRequestResponseDto> ReceiveMaterialRequestAsync(ErpWorkstationMaterialRequestRequestDto request)
        {
            try
            {
                // 验证请求数据
                if (request == null)
                    return new ErpWorkstationMaterialRequestResponseDto { Succeed = false, Message = "请求数据不能为空" };

                // 检查分拣批次是否已存在
                var existingRequest = await _erpWorkstationMaterialRequestRepository.ExistsBySortingBatchAsync(request.SortingBatch);
                if (existingRequest)
                    return new ErpWorkstationMaterialRequestResponseDto { Succeed = false, Message = $"分拣批次{request.SortingBatch}已存在，不能重复创建" };

                // 创建叫料任务
                var materialRequest = await _erpWorkstationMaterialRequestManager.CreateMaterialRequestAsync(
                    request.SortingBatch,
                    request.DeliveryPointLocation,
                    request.DeliveryTime);

                // 保存到数据库
                await _erpWorkstationMaterialRequestRepository.InsertAsync(materialRequest);

                _logger.LogInformation($"成功接收ERP工位叫料任务：分拣批次{request.SortingBatch}，配送点{request.DeliveryPointLocation}，配送时间{request.DeliveryTime:yyyy-MM-dd HH:mm:ss}");

                return new ErpWorkstationMaterialRequestResponseDto
                {
                    Succeed = true,
                    Message = "叫料任务接收成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP工位叫料任务失败：{ex.Message}");
                return new ErpWorkstationMaterialRequestResponseDto
                {
                    Succeed = false,
                    Message = $"接收叫料任务失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 根据ID获取叫料任务
        /// </summary>
        public async Task<ErpWorkstationMaterialRequestDto> GetAsync(Guid id)
        {
            var materialRequest = await _erpWorkstationMaterialRequestRepository.GetAsync(id);
            return ObjectMapper.Map<ErpWorkstationMaterialRequest, ErpWorkstationMaterialRequestDto>(materialRequest);
        }

        /// <summary>
        /// 根据分拣批次获取叫料任务
        /// </summary>
        public async Task<ErpWorkstationMaterialRequestDto> GetBySortingBatchAsync(string sortingBatch)
        {
            var materialRequest = await _erpWorkstationMaterialRequestRepository.FindBySortingBatchAsync(sortingBatch);
            if (materialRequest == null)
                return null;

            return ObjectMapper.Map<ErpWorkstationMaterialRequest, ErpWorkstationMaterialRequestDto>(materialRequest);
        }

        /// <summary>
        /// 获取叫料任务列表
        /// </summary>
        public async Task<PagedResultDto<ErpWorkstationMaterialRequestDto>> GetListAsync(ErpWorkstationMaterialRequestQueryDto input)
        {
            var query = await _erpWorkstationMaterialRequestRepository.GetQueryableAsync();

            // 应用过滤条件
            var filteredQuery = query;
            if (!string.IsNullOrWhiteSpace(input.SortingBatch))
                filteredQuery = filteredQuery.Where(x => x.SortingBatch.Contains(input.SortingBatch));
            if (!string.IsNullOrWhiteSpace(input.DeliveryPointLocation))
                filteredQuery = filteredQuery.Where(x => x.DeliveryPointLocation.Contains(input.DeliveryPointLocation));
            if (input.Status.HasValue)
                filteredQuery = filteredQuery.Where(x => x.Status == (MaterialRequestStatus)input.Status.Value);
            if (input.StartTime.HasValue)
                filteredQuery = filteredQuery.Where(x => x.DeliveryTime >= input.StartTime.Value);
            if (input.EndTime.HasValue)
                filteredQuery = filteredQuery.Where(x => x.DeliveryTime <= input.EndTime.Value);

            // 应用排序
            var sortedQuery = input.Sorting.IsNullOrWhiteSpace() 
                ? filteredQuery.OrderByDescending(x => x.CreationTime)
                : filteredQuery.OrderBy(x => input.Sorting);

            // 应用分页
            var totalCount = await sortedQuery.CountAsync();
            var items = await sortedQuery.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

            var dtos = ObjectMapper.Map<List<ErpWorkstationMaterialRequest>, List<ErpWorkstationMaterialRequestDto>>(items);

            return new PagedResultDto<ErpWorkstationMaterialRequestDto>
            {
                TotalCount = totalCount,
                Items = dtos
            };
        }

        /// <summary>
        /// 根据配送点位置获取叫料任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetByDeliveryPointLocationAsync(string deliveryPointLocation)
        {
            var materialRequests = await _erpWorkstationMaterialRequestRepository.FindByDeliveryPointLocationAsync(deliveryPointLocation);
            return ObjectMapper.Map<List<ErpWorkstationMaterialRequest>, List<ErpWorkstationMaterialRequestDto>>(materialRequests);
        }

        /// <summary>
        /// 根据状态获取叫料任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetByStatusAsync(int status)
        {
            var materialRequests = await _erpWorkstationMaterialRequestRepository.FindByStatusAsync((MaterialRequestStatus)status);
            return ObjectMapper.Map<List<ErpWorkstationMaterialRequest>, List<ErpWorkstationMaterialRequestDto>>(materialRequests);
        }

        /// <summary>
        /// 更新叫料任务状态
        /// </summary>
        [UnitOfWork]
        public async Task<bool> UpdateStatusAsync(Guid id, ErpWorkstationMaterialRequestStatusUpdateDto input)
        {
            try
            {
                var materialRequest = await _erpWorkstationMaterialRequestRepository.GetAsync(id);
                
                // 验证状态转换是否有效
                if (!_erpWorkstationMaterialRequestManager.IsValidStatusTransition(materialRequest.Status, (MaterialRequestStatus)input.Status))
                    throw new Exception($"状态从{materialRequest.Status}转换到{(MaterialRequestStatus)input.Status}无效");

                // 更新状态
                _erpWorkstationMaterialRequestManager.UpdateRequestStatus(materialRequest, (MaterialRequestStatus)input.Status);

                // 添加备注
                if (!string.IsNullOrWhiteSpace(input.Remarks))
                {
                    _erpWorkstationMaterialRequestManager.AddRemarks(materialRequest, input.Remarks);
                }

                await _erpWorkstationMaterialRequestRepository.UpdateAsync(materialRequest);

                _logger.LogInformation($"成功更新叫料任务{id}状态为{(MaterialRequestStatus)input.Status}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新叫料任务{id}状态失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除叫料任务
        /// </summary>
        [UnitOfWork]
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await _erpWorkstationMaterialRequestRepository.DeleteAsync(id);
                _logger.LogInformation($"成功删除叫料任务{id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除叫料任务{id}失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取待处理的叫料任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequestDto>> GetPendingRequestsAsync()
        {
            var materialRequests = await _erpWorkstationMaterialRequestRepository.GetPendingRequestsAsync();
            return ObjectMapper.Map<List<ErpWorkstationMaterialRequest>, List<ErpWorkstationMaterialRequestDto>>(materialRequests);
        }
    }
}
