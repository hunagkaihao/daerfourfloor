using System;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Erp.IDto;

namespace TuTa.Wms.Application.Contracts.Erp
{
    public interface IErpDeliveryOrderAppService
    {
        Task<ErpDeliveryOrderListResponseDto> GetDeliveryOrderListAsync(
            int page,
            int pageSize,
            string deliveryOrderNo = null,
            string warehouseCode = null,
            string startDate = null,
            string endDate = null);

        Task<ErpDeliveryOrderDto> GetDeliveryOrderByIdAsync(Guid id);

        Task<ErpDeliveryOrderDto> CreateDeliveryOrderAsync(ErpDeliveryOrderCreateDto input);

        Task<ErpDeliveryOrderDto> UpdateDeliveryOrderAsync(Guid id, ErpDeliveryOrderCreateDto input);

        Task DeleteDeliveryOrderAsync(Guid id);

        Task<ErpDeliveryOrderDto> CompleteDeliveryOrderAsync(Guid id);
    }
}