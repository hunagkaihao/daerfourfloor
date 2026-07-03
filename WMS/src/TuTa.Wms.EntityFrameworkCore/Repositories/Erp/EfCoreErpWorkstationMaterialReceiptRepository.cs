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
    /// ERP工位收料EF Core仓储实现
    /// </summary>
    public class EfCoreErpWorkstationMaterialReceiptRepository : EfCoreRepository<ErpDbContext, ErpWorkstationMaterialReceipt, Guid>, IErpWorkstationMaterialReceiptRepository
    {
        public EfCoreErpWorkstationMaterialReceiptRepository(IDbContextProvider<ErpDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据分拣批次号查找收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>收料记录</returns>
        public async Task<ErpWorkstationMaterialReceipt> FindBySortingBatchAsync(string sortingBatch)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ErpWorkstationMaterialReceipts
                .FirstOrDefaultAsync(x => x.SortingBatch == sortingBatch);
        }

        /// <summary>
        /// 根据收料时间范围查找收料记录
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>收料记录列表</returns>
        public async Task<List<ErpWorkstationMaterialReceipt>> FindByReceiptTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ErpWorkstationMaterialReceipts
                .Where(x => x.ReceiptTime >= startTime && x.ReceiptTime <= endTime)
                .OrderByDescending(x => x.ReceiptTime)
                .ToListAsync();
        }

        /// <summary>
        /// 检查分拣批次是否已收料
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <returns>是否已收料</returns>
        public async Task<bool> ExistsBySortingBatchAsync(string sortingBatch)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ErpWorkstationMaterialReceipts
                .AnyAsync(x => x.SortingBatch == sortingBatch);
        }
    }
}
