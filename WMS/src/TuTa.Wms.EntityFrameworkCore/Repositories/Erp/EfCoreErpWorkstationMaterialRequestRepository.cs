using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using TuTa.Wms.EntityFrameworkCore;

namespace TuTa.Wms.EntityFrameworkCore.Repositories.Erp
{
    /// <summary>
    /// ERP工位叫料任务EF Core仓储实现
    /// </summary>
    public class EfCoreErpWorkstationMaterialRequestRepository : EfCoreRepository<ErpDbContext, ErpWorkstationMaterialRequest, Guid>, IErpWorkstationMaterialRequestRepository
    {
        public EfCoreErpWorkstationMaterialRequestRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据分拣批次查找物料请求任务
        /// </summary>
        public async Task<ErpWorkstationMaterialRequest> FindBySortingBatchAsync(string sortingBatch)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.SortingBatch == sortingBatch)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据配送点位置查找物料请求任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequest>> FindByDeliveryPointLocationAsync(string deliveryPointLocation)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.DeliveryPointLocation == deliveryPointLocation)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态查找物料请求任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequest>> FindByStatusAsync(MaterialRequestStatus status)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.Status == status)
                .OrderBy(o => o.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据配送时间范围查找物料请求任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequest>> FindByDeliveryTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.DeliveryTime >= startTime && o.DeliveryTime <= endTime)
                .OrderBy(o => o.DeliveryTime)
                .ToListAsync();
        }

        /// <summary>
        /// 获取待处理的物料请求任务列表
        /// </summary>
        public async Task<List<ErpWorkstationMaterialRequest>> GetPendingRequestsAsync()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(o => o.Status == MaterialRequestStatus.Created || o.Status == MaterialRequestStatus.Processing)
                .OrderBy(o => o.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 检查分拣批次是否已存在
        /// </summary>
        public async Task<bool> ExistsBySortingBatchAsync(string sortingBatch)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .AnyAsync(o => o.SortingBatch == sortingBatch);
        }
    }
}
