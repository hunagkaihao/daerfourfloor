using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace TuTa.Wms.Erp.Events
{
    public class StockInCompletedEventHandler : ILocalEventHandler<StockInCompletedEvent>, ITransientDependency
    {
        private readonly IErpAsnStockInService _erpAsnStockInService;
        private readonly ILogger<StockInCompletedEventHandler> _logger;

        public StockInCompletedEventHandler(
            IErpAsnStockInService erpAsnStockInService,
            ILogger<StockInCompletedEventHandler> logger)
        {
            _erpAsnStockInService = erpAsnStockInService;
            _logger = logger;
        }

        public async Task HandleEventAsync(StockInCompletedEvent eventData)
        {
            _logger.LogInformation($"收到入库完成事件，ASN码：{eventData.AsnCode}，入库数量：{eventData.StockInQuantity}");
            
            try
            {
                await _erpAsnStockInService.HandleStockInCompletedAsync(eventData.AsnCode, eventData.StockInQuantity);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"处理入库完成事件失败，ASN码：{eventData.AsnCode}");
            }
        }
    }
}