using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    /// ERP物料仓储实现
    /// </summary>
    public class ErpMaterialRepository : EfCoreRepository<WmsDbContext, ErpMaterial, Guid>, IErpMaterialRepository
    {
        public ErpMaterialRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据物料代号查找物料
        /// </summary>
        public async Task<ErpMaterial> FindByMaterialCodeAsync(string materialCode)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpMaterial>()
                .FirstOrDefaultAsync(m => m.MaterialCode == materialCode);
        }

        /// <summary>
        /// 根据操作类型查找物料列表
        /// </summary>
        public async Task<List<ErpMaterial>> GetBySyncTypeAsync(string syncType)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpMaterial>()
                .Where(m => m.SyncType == syncType)
                .ToListAsync();
        }

        /// <summary>
        /// 根据同步时间戳范围查找物料列表
        /// </summary>
        public async Task<List<ErpMaterial>> GetBySyncTimeStampRangeAsync(long startTimeStamp, long endTimeStamp)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpMaterial>()
                .Where(m => m.SyncTimeStamp >= startTimeStamp && m.SyncTimeStamp <= endTimeStamp)
                .ToListAsync();
        }

        /// <summary>
        /// 获取所有未接收的物料
        /// </summary>
        public async Task<List<ErpMaterial>> GetAllUnReceivedMaterialsAsync(CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpMaterial>()
                .Where(m => m.SyncType == "NEW") // 新物料
                .ToListAsync(cancellationToken);
        }
    }
}
