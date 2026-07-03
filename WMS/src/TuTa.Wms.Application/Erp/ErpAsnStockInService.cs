using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public interface IErpAsnStockInService
    {
        Task HandleStockInCompletedAsync(string asnCode, decimal stockInQuantity);
        
        Task<bool> CheckAndPushToErpAsync(string asnCode);
    }

    public class ErpAsnStockInService : ApplicationService, IErpAsnStockInService
    {
        private readonly IErpAsnRepository _erpAsnRepository;
        private readonly ErpAsnAppService _erpAsnAppService;
        private readonly ILogger<ErpAsnStockInService> _logger;

        public ErpAsnStockInService(
            IErpAsnRepository erpAsnRepository,
            ErpAsnAppService erpAsnAppService,
            ILogger<ErpAsnStockInService> logger)
        {
            _erpAsnRepository = erpAsnRepository;
            _erpAsnAppService = erpAsnAppService;
            _logger = logger;
        }

        [UnitOfWork]
        public async Task HandleStockInCompletedAsync(string asnCode, decimal stockInQuantity)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 开始处理入库完成，ASN码：{asnCode}，入库数量：{stockInQuantity}");

            try
            {
                var erpAsn = await _erpAsnRepository.GetByAsnCodeAsync(asnCode);
                if (erpAsn == null)
                {
                    _logger.LogWarning($"[操作ID: {operationId}] 未找到ASN码：{asnCode}，跳过处理");
                    return;
                }

                _logger.LogInformation($"[操作ID: {operationId}] 更新ASN入库数量，当前已入库：{erpAsn.StockInQuantity}，新增：{stockInQuantity}");
                erpAsn.AddStockInQuantity(stockInQuantity);
                await _erpAsnRepository.UpdateAsync(erpAsn);

                if (erpAsn.IsStockInCompleted())
                {
                    _logger.LogInformation($"[操作ID: {operationId}] ASN码：{asnCode} 已全部入库，开始推送ERP做到货单");
                    await CheckAndPushToErpAsync(asnCode);
                }
                else
                {
                    _logger.LogInformation($"[操作ID: {operationId}] ASN码：{asnCode} 未完成入库，已入库：{erpAsn.StockInQuantity}/{erpAsn.PlanQuantity}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[操作ID: {operationId}] 处理入库完成异常，ASN码：{asnCode}");
                throw;
            }
        }

        [UnitOfWork]
        public async Task<bool> CheckAndPushToErpAsync(string asnCode)
        {
            var operationId = Guid.NewGuid().ToString();
            _logger.LogInformation($"[操作ID: {operationId}] 检查并推送ERP做到货单，ASN码：{asnCode}");

            try
            {
                var erpAsn = await _erpAsnRepository.GetByAsnCodeAsync(asnCode);
                if (erpAsn == null)
                {
                    _logger.LogWarning($"[操作ID: {operationId}] 未找到ASN码：{asnCode}");
                    return false;
                }

                if (!erpAsn.IsStockInCompleted())
                {
                    _logger.LogWarning($"[操作ID: {operationId}] ASN码：{asnCode} 未完成入库，已入库：{erpAsn.StockInQuantity}/{erpAsn.PlanQuantity}");
                    return false;
                }

                if (erpAsn.IsPushedToErp)
                {
                    _logger.LogInformation($"[操作ID: {operationId}] ASN码：{asnCode} 已推送过ERP，跳过");
                    return true;
                }

                _logger.LogInformation($"[操作ID: {operationId}] 开始推送ERP做到货单，ASN码：{asnCode}");
                bool pushResult = await _erpAsnAppService.PushErpReceiptAsync(asnCode);

                if (pushResult)
                {
                    _logger.LogInformation($"[操作ID: {operationId}] 成功推送ERP做到货单，ASN码：{asnCode}");
                    erpAsn.MarkAsPushedToErp();
                    await _erpAsnRepository.UpdateAsync(erpAsn);
                    return true;
                }
                else
                {
                    _logger.LogError($"[操作ID: {operationId}] 推送ERP做到货单失败，ASN码：{asnCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[操作ID: {operationId}] 推送ERP做到货单异常，ASN码：{asnCode}");
                return false;
            }
        }
    }
}