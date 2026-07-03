using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Repositories;
using TuTa.Wms.Erp.Aggregates;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP车间物料转移应用服务实现
    /// </summary>
    public class ErpWorkshopMaterialTransferAppService : ApplicationService, IErpWorkshopMaterialTransferAppService
    {
        private readonly IErpWorkshopMaterialTransferRepository _erpWorkshopMaterialTransferRepository;
        private readonly ErpWorkshopMaterialTransferManager _erpWorkshopMaterialTransferManager;
        private readonly ILogger<ErpWorkshopMaterialTransferAppService> _logger;

        public ErpWorkshopMaterialTransferAppService(
            IErpWorkshopMaterialTransferRepository erpWorkshopMaterialTransferRepository,
            ErpWorkshopMaterialTransferManager erpWorkshopMaterialTransferManager,
            ILogger<ErpWorkshopMaterialTransferAppService> logger)
        {
            _erpWorkshopMaterialTransferRepository = erpWorkshopMaterialTransferRepository;
            _erpWorkshopMaterialTransferManager = erpWorkshopMaterialTransferManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP车间移库AGV任务
        /// </summary>
        /// <param name="request">转移任务请求</param>
        /// <returns>处理结果</returns>
        public async Task<ErpWorkshopMaterialTransferResponseDto> ReceiveMaterialTransferTaskAsync(ErpWorkshopMaterialTransferRequestDto request)
        {
            try
            {
                // 使用领域服务创建转移任务
                var materialTransfer = await _erpWorkshopMaterialTransferManager.CreateMaterialTransferTaskAsync(
                    request.StartLocation, 
                    request.EndLocation);

                // 保存到数据库
                await _erpWorkshopMaterialTransferRepository.InsertAsync(materialTransfer);

                _logger.LogInformation($"成功接收ERP车间移库AGV任务：从 {request.StartLocation} 到 {request.EndLocation}");

                return new ErpWorkshopMaterialTransferResponseDto
                {
                    Succeed = true,
                    Message = "ERP车间移库AGV任务接收成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP车间移库AGV任务失败：从 {request.StartLocation} 到 {request.EndLocation}");

                return new ErpWorkshopMaterialTransferResponseDto
                {
                    Succeed = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 根据ID获取转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>转移任务信息</returns>
        public async Task<ErpWorkshopMaterialTransferDto> GetAsync(Guid id)
        {
            var entity = await _erpWorkshopMaterialTransferRepository.GetAsync(id);
            var dto = ObjectMapper.Map<ErpWorkshopMaterialTransfer, ErpWorkshopMaterialTransferDto>(entity);
            dto.StatusDescription = GetStatusDescription((MaterialTransferStatus)entity.Status);
            return dto;
        }

        /// <summary>
        /// 根据启动位置获取转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransferDto>> GetByStartLocationAsync(string startLocation)
        {
            var entities = await _erpWorkshopMaterialTransferRepository.FindByStartLocationAsync(startLocation);
            var dtos = ObjectMapper.Map<List<ErpWorkshopMaterialTransfer>, List<ErpWorkshopMaterialTransferDto>>(entities);
            
            foreach (var dto in dtos)
            {
                dto.StatusDescription = GetStatusDescription((MaterialTransferStatus)dto.Status);
            }
            
            return dtos;
        }

        /// <summary>
        /// 根据终点位置获取转移任务
        /// </summary>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransferDto>> GetByEndLocationAsync(string endLocation)
        {
            var entities = await _erpWorkshopMaterialTransferRepository.FindByEndLocationAsync(endLocation);
            var dtos = ObjectMapper.Map<List<ErpWorkshopMaterialTransfer>, List<ErpWorkshopMaterialTransferDto>>(entities);
            
            foreach (var dto in dtos)
            {
                dto.StatusDescription = GetStatusDescription((MaterialTransferStatus)dto.Status);
            }
            
            return dtos;
        }

        /// <summary>
        /// 获取转移任务列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>转移任务列表</returns>
        public async Task<PagedResultDto<ErpWorkshopMaterialTransferDto>> GetListAsync(ErpWorkshopMaterialTransferQueryDto input)
        {
            var query = await _erpWorkshopMaterialTransferRepository.GetQueryableAsync();

            // 应用查询条件
            if (!string.IsNullOrWhiteSpace(input.StartLocation))
            {
                query = query.Where(x => x.StartLocation.Contains(input.StartLocation));
            }

            if (!string.IsNullOrWhiteSpace(input.EndLocation))
            {
                query = query.Where(x => x.EndLocation.Contains(input.EndLocation));
            }

            if (input.Status.HasValue)
            {
                query = query.Where(x => x.Status == (MaterialTransferStatus)input.Status.Value);
            }

            if (input.StartTime.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                query = query.Where(x => x.CreationTime <= input.EndTime.Value);
            }

            // 排序
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                // 使用动态排序，需要确保字段名正确
                if (input.Sorting.ToLower() == "startlocation")
                {
                    query = query.OrderBy(x => x.StartLocation);
                }
                else if (input.Sorting.ToLower() == "endlocation")
                {
                    query = query.OrderBy(x => x.EndLocation);
                }
                else if (input.Sorting.ToLower() == "status")
                {
                    query = query.OrderBy(x => x.Status);
                }
                else if (input.Sorting.ToLower() == "creationtime")
                {
                    query = query.OrderBy(x => x.CreationTime);
                }
                else
                {
                    query = query.OrderByDescending(x => x.CreationTime);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            // 分页
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var dtos = ObjectMapper.Map<List<ErpWorkshopMaterialTransfer>, List<ErpWorkshopMaterialTransferDto>>(items);
            
            foreach (var dto in dtos)
            {
                dto.StatusDescription = GetStatusDescription((MaterialTransferStatus)dto.Status);
            }

            return new PagedResultDto<ErpWorkshopMaterialTransferDto>
            {
                TotalCount = totalCount,
                Items = dtos
            };
        }

        /// <summary>
        /// 更新转移任务状态
        /// </summary>
        /// <param name="input">状态更新请求</param>
        /// <returns>更新结果</returns>
        public async Task<ErpWorkshopMaterialTransferResponseDto> UpdateStatusAsync(ErpWorkshopMaterialTransferStatusUpdateDto input)
        {
            try
            {
                var newStatus = (MaterialTransferStatus)input.Status;
                await _erpWorkshopMaterialTransferManager.UpdateTaskStatusAsync(input.Id, newStatus);

                _logger.LogInformation($"成功更新转移任务{input.Id}状态为{newStatus}");

                return new ErpWorkshopMaterialTransferResponseDto
                {
                    Succeed = true,
                    Message = $"任务状态更新成功：{GetStatusDescription(newStatus)}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新转移任务{input.Id}状态失败");

                return new ErpWorkshopMaterialTransferResponseDto
                {
                    Succeed = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 删除转移任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>删除结果</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            await _erpWorkshopMaterialTransferRepository.DeleteAsync(id);
            _logger.LogInformation($"成功删除转移任务{id}");
            return true;
        }

        /// <summary>
        /// 获取待处理的转移任务数量
        /// </summary>
        /// <returns>待处理任务数量</returns>
        public async Task<int> GetPendingTaskCountAsync()
        {
            return await _erpWorkshopMaterialTransferRepository.GetPendingTaskCountAsync();
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        /// <param name="status">状态枚举</param>
        /// <returns>状态描述</returns>
        private string GetStatusDescription(MaterialTransferStatus status)
        {
            return status switch
            {
                MaterialTransferStatus.Pending => "待处理",
                MaterialTransferStatus.InProgress => "执行中",
                MaterialTransferStatus.Completed => "已完成",
                MaterialTransferStatus.Cancelled => "已取消",
                MaterialTransferStatus.Failed => "执行失败",
                _ => "未知状态"
            };
        }
    }
}
