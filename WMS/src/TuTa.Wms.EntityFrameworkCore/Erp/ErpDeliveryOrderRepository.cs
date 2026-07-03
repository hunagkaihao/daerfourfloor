using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Erp
{
    public class ErpDeliveryOrderRepository : EfCoreRepository<WmsDbContext, ErpDeliveryOrder, Guid>, IErpDeliveryOrderRepository
    {
        public ErpDeliveryOrderRepository(IDbContextProvider<WmsDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<(List<ErpDeliveryOrder>, int)> GetDeliveryOrdersAsync(
            int page,
            int pageSize,
            string deliveryOrderNo = null,
            string warehouseCode = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var dbContext = await GetDbContextAsync();

            var query = dbContext.ErpDeliveryOrders.AsQueryable();

            if (!string.IsNullOrEmpty(deliveryOrderNo))
            {
                query = query.Where(o => o.DeliveryOrderNo.Contains(deliveryOrderNo));
            }

            if (!string.IsNullOrEmpty(warehouseCode))
            {
                query = query.Where(o => o.WarehouseCode == warehouseCode);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate <= endDate.Value);
            }

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, total);
        }

        public async Task<ErpDeliveryOrder> GetByIdWithItemsAsync(Guid id)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.ErpDeliveryOrders
                .Include(o => o.Id)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<ErpDeliveryOrderItem>> GetItemsByOrderIdAsync(Guid deliveryOrderId)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.ErpDeliveryOrderItems
                .Where(i => i.DeliveryOrderId == deliveryOrderId)
                .ToListAsync();
        }

        public async Task<bool> ExistsByOrderNoAsync(string deliveryOrderNo)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.ErpDeliveryOrders
                .AnyAsync(o => o.DeliveryOrderNo == deliveryOrderNo);
        }
    }

    public class ErpDeliveryOrderItemRepository : EfCoreRepository<WmsDbContext, ErpDeliveryOrderItem, Guid>, IErpDeliveryOrderItemRepository
    {
        public ErpDeliveryOrderItemRepository(IDbContextProvider<WmsDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<ErpDeliveryOrderItem>> GetByOrderIdAsync(Guid deliveryOrderId)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.ErpDeliveryOrderItems
                .Where(i => i.DeliveryOrderId == deliveryOrderId)
                .ToListAsync();
        }
    }
}