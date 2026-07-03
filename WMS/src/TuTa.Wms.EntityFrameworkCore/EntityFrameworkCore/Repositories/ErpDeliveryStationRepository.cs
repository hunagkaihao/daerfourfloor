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

namespace TuTa.Wms.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// ERP收料工位仓储实现
    /// </summary>
    public class ErpDeliveryStationRepository : EfCoreRepository<WmsDbContext, ErpDeliveryStation, Guid>, IErpDeliveryStationRepository
    {
        public ErpDeliveryStationRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据配送位置代号查找收料工位
        /// </summary>
        public async Task<ErpDeliveryStation> FindByDeliveryCodeAsync(string deliveryCode)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpDeliveryStation>()
                .FirstOrDefaultAsync(d => d.DeliveryCode == deliveryCode);
        }

        /// <summary>
        /// 根据操作类型查找收料工位列表
        /// </summary>
        public async Task<List<ErpDeliveryStation>> GetBySyncTypeAsync(string syncType)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpDeliveryStation>()
                .Where(d => d.SyncType == syncType)
                .ToListAsync();
        }

        /// <summary>
        /// 根据同步时间戳范围查找收料工位列表
        /// </summary>
        public async Task<List<ErpDeliveryStation>> GetBySyncTimeStampRangeAsync(long startTimeStamp, long endTimeStamp)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpDeliveryStation>()
                .Where(d => d.SyncTimeStamp >= startTimeStamp && d.SyncTimeStamp <= endTimeStamp)
                .ToListAsync();
        }
    }
}
