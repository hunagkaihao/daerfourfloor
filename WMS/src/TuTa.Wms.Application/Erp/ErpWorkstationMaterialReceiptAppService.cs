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
    /// ERP工位收料应用服务实现
    /// </summary>
    public class ErpWorkstationMaterialReceiptAppService : ApplicationService, IErpWorkstationMaterialReceiptAppService
    {
        private readonly IErpWorkstationMaterialReceiptRepository _erpWorkstationMaterialReceiptRepository;
        private readonly ErpWorkstationMaterialReceiptManager _erpWorkstationMaterialReceiptManager;
        private readonly ILogger<ErpWorkstationMaterialReceiptAppService> _logger;

        public ErpWorkstationMaterialReceiptAppService(
            IErpWorkstationMaterialReceiptRepository erpWorkstationMaterialReceiptRepository,
            ErpWorkstationMaterialReceiptManager erpWorkstationMaterialReceiptManager,
            ILogger<ErpWorkstationMaterialReceiptAppService> logger)
        {
            _erpWorkstationMaterialReceiptRepository = erpWorkstationMaterialReceiptRepository;
            _erpWorkstationMaterialReceiptManager = erpWorkstationMaterialReceiptManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收工位收料信息
        /// </summary>
        /// <param name="request">收料请求数据</param>
        /// <returns>处理结果</returns>
        public async Task<ErpWorkstationMaterialReceiptResponseDto> ReceiveMaterialReceiptAsync(ErpWorkstationMaterialReceiptRequestDto request)
        {
            try
            {
                // 使用领域服务创建收料记录
                var materialReceipt = await _erpWorkstationMaterialReceiptManager.CreateMaterialReceiptAsync(
                    request.SortingBatch, 
                    request.ReceiptTime);

                // 保存到数据库
                await _erpWorkstationMaterialReceiptRepository.InsertAsync(materialReceipt);

                _logger.LogInformation($"成功接收工位收料信息：分拣批次{request.SortingBatch}，收料时间{request.ReceiptTime:yyyy-MM-dd HH:mm:ss}");

                return new ErpWorkstationMaterialReceiptResponseDto
                {
                    Succeed = true,
                    Message = "工位收料信息接收成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收工位收料信息失败：分拣批次{request.SortingBatch}，收料时间{request.ReceiptTime:yyyy-MM-dd HH:mm:ss}");

                return new ErpWorkstationMaterialReceiptResponseDto
                {
                    Succeed = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 根据ID获取收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>收料记录信息</returns>
        public async Task<ErpWorkstationMaterialReceiptDto> GetAsync(Guid id)
        {
            var entity = await _erpWorkstationMaterialReceiptRepository.GetAsync(id);
            return ObjectMapper.Map<ErpWorkstationMaterialReceipt, ErpWorkstationMaterialReceiptDto>(entity);
        }

        /// <summary>
        /// 根据分拣批次号获取收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>收料记录信息</returns>
        public async Task<ErpWorkstationMaterialReceiptDto> GetBySortingBatchAsync(string sortingBatch)
        {
            var entity = await _erpWorkstationMaterialReceiptRepository.FindBySortingBatchAsync(sortingBatch);
            if (entity == null)
            {
                return null;
            }
            return ObjectMapper.Map<ErpWorkstationMaterialReceipt, ErpWorkstationMaterialReceiptDto>(entity);
        }

        /// <summary>
        /// 获取收料记录列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>收料记录列表</returns>
        public async Task<PagedResultDto<ErpWorkstationMaterialReceiptDto>> GetListAsync(ErpWorkstationMaterialReceiptQueryDto input)
        {
            var query = await _erpWorkstationMaterialReceiptRepository.GetQueryableAsync();

            // 应用查询条件
            if (!string.IsNullOrWhiteSpace(input.SortingBatch))
            {
                query = query.Where(x => x.SortingBatch.Contains(input.SortingBatch));
            }

            if (input.StartTime.HasValue)
            {
                query = query.Where(x => x.ReceiptTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                query = query.Where(x => x.ReceiptTime <= input.EndTime.Value);
            }

            // 排序
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                // 使用动态排序，需要确保字段名正确
                if (input.Sorting.ToLower() == "sortingbatch")
                {
                    query = query.OrderBy(x => x.SortingBatch);
                }
                else if (input.Sorting.ToLower() == "receipttime")
                {
                    query = query.OrderBy(x => x.ReceiptTime);
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

            var dtos = ObjectMapper.Map<List<ErpWorkstationMaterialReceipt>, List<ErpWorkstationMaterialReceiptDto>>(items);

            return new PagedResultDto<ErpWorkstationMaterialReceiptDto>
            {
                TotalCount = totalCount,
                Items = dtos
            };
        }

        /// <summary>
        /// 根据时间范围获取收料记录列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>收料记录列表</returns>
        public async Task<List<ErpWorkstationMaterialReceiptDto>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var entities = await _erpWorkstationMaterialReceiptRepository.FindByReceiptTimeRangeAsync(startTime, endTime);
            return ObjectMapper.Map<List<ErpWorkstationMaterialReceipt>, List<ErpWorkstationMaterialReceiptDto>>(entities);
        }

        /// <summary>
        /// 删除收料记录
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <returns>删除结果</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            await _erpWorkstationMaterialReceiptRepository.DeleteAsync(id);
            return true;
        }
    }
}
