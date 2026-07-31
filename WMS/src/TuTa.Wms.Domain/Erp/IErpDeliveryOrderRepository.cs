using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp
{
    public interface IErpoutboundRepository : IRepository<ErpDeliveryOrder, Guid>
    {
        Task<(List<ErpDeliveryOrder>, int)> GetDeliveryOrdersAsync(
            int page,
            int pageSize,
            string deliveryOrderNo = null,
            string warehouseCode = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ErpDeliveryOrder> GetByIdWithItemsAsync(Guid id);

        Task<List<ErpDeliveryOrderItem>> GetItemsByOrderIdAsync(Guid deliveryOrderId);

        Task<bool> ExistsByOrderNoAsync(string deliveryOrderNo);

        Task<ErpDeliveryOrder> FindByOrderNoAsync(string deliveryOrderNo);
    }

    public interface IErpDeliveryOrderItemRepository : IRepository<ErpDeliveryOrderItem, Guid>
    {
        Task<List<ErpDeliveryOrderItem>> GetByOrderIdAsync(Guid deliveryOrderId);
    }
}