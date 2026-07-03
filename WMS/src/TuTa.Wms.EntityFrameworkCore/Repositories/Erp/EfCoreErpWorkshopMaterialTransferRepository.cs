using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.EntityFrameworkCore.Repositories.Erp
{
    /// <summary>
    /// ERP车间物料转移EF Core仓储实现
    /// </summary>
    public class EfCoreErpWorkshopMaterialTransferRepository : EfCoreRepository<ErpDbContext, ErpWorkshopMaterialTransfer, Guid>, IErpWorkshopMaterialTransferRepository
    {
        public EfCoreErpWorkshopMaterialTransferRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据启动位置查找转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransfer>> FindByStartLocationAsync(string startLocation)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpWorkshopMaterialTransfer>()
                .Where(x => x.StartLocation.Contains(startLocation))
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据终点位置查找转移任务
        /// </summary>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransfer>> FindByEndLocationAsync(string endLocation)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpWorkshopMaterialTransfer>()
                .Where(x => x.EndLocation.Contains(endLocation))
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态查找转移任务
        /// </summary>
        /// <param name="status">任务状态</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransfer>> FindByStatusAsync(MaterialTransferStatus status)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpWorkshopMaterialTransfer>()
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据位置范围查找转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <param name="endLocation">终点位置</param>
        /// <returns>转移任务列表</returns>
        public async Task<List<ErpWorkshopMaterialTransfer>> FindByLocationsAsync(string startLocation, string endLocation)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpWorkshopMaterialTransfer>()
                .Where(x => x.StartLocation == startLocation && x.EndLocation == endLocation)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 获取待处理的转移任务数量
        /// </summary>
        /// <returns>待处理任务数量</returns>
        public async Task<int> GetPendingTaskCountAsync()
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpWorkshopMaterialTransfer>()
                .Where(x => x.Status == MaterialTransferStatus.Pending)
                .CountAsync();
        }
    }
}
