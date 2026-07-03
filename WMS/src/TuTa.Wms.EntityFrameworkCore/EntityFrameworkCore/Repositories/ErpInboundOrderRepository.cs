using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// ERP入库单仓储实现
    /// </summary>
    public class ErpInboundOrderRepository : EfCoreRepository<WmsDbContext, ErpInboundOrder, Guid>, IErpInboundOrderRepository
    {
        public ErpInboundOrderRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据入库单号查找入库单
        /// </summary>
        public async Task<ErpInboundOrder> FindByInboundOrderNoAsync(string inboundOrderNo)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .FirstOrDefaultAsync(x => x.InboundOrderNo == inboundOrderNo);
        }

        /// <summary>
        /// 根据仓库代号查找入库单列表
        /// </summary>
        public async Task<List<ErpInboundOrder>> GetByWarehouseCodeAsync(string warehouseCode)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .Where(x => x.WarehouseCode == warehouseCode)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态查找入库单列表
        /// </summary>
        public async Task<List<ErpInboundOrder>> GetByStatusAsync(InboundOrderStatus status)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据计划入库日期范围查找入库单列表
        /// </summary>
        public async Task<List<ErpInboundOrder>> GetByPlanInboundDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .Where(x => x.PlanInboundDate >= startDate && x.PlanInboundDate <= endDate)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 获取入库单列表，包含入库单项
        /// </summary>
        public async Task<List<ErpInboundOrder>> GetListWithItemsAsync()
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取入库单，包含入库单项
        /// </summary>
        public async Task<ErpInboundOrder> GetWithItemsAsync(Guid id)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpInboundOrder>()
                .Include(x => x.InboundItems)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
