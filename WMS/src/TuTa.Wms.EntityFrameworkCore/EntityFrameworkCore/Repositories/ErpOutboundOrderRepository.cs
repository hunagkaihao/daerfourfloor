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
    /// ERP出库单仓储实现
    /// </summary>
    public class ErpOutboundOrderRepository : EfCoreRepository<WmsDbContext, ErpOutboundOrder, Guid>, IErpOutboundOrderRepository
    {
        public ErpOutboundOrderRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        /// <summary>
        /// 根据出库单号查找出库单
        /// </summary>
        public async Task<ErpOutboundOrder> FindByOutboundOrderNoAsync(string outboundOrderNo)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .FirstOrDefaultAsync(o => o.OutboundOrderNo == outboundOrderNo);
        }

        /// <summary>
        /// 根据仓库代号查找出库单列表
        /// </summary>
        public async Task<List<ErpOutboundOrder>> GetByWarehouseCodeAsync(string warehouseCode)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .Where(o => o.WarehouseCode == warehouseCode)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态查找出库单列表
        /// </summary>
        public async Task<List<ErpOutboundOrder>> GetByStatusAsync(OutboundOrderStatus status)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .Where(o => o.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// 根据计划出库日期范围查找出库单列表
        /// </summary>
        public async Task<List<ErpOutboundOrder>> GetByPlanOutboundDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .Where(o => o.PlanOutboundDate >= startDate && o.PlanOutboundDate <= endDate)
                .ToListAsync();
        }

        /// <summary>
        /// 获取出库单列表，包含出库单项
        /// </summary>
        public async Task<List<ErpOutboundOrder>> GetListWithItemsAsync()
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .Include(o => o.OutboundItems)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取出库单，包含出库单项
        /// </summary>
        public async Task<ErpOutboundOrder> GetWithItemsAsync(Guid id)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.Set<ErpOutboundOrder>()
                .Include(o => o.OutboundItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
