using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.PickLists;
using TuTa.Wms.PickLists.ValueObjects;
using TuTa.Wms.Stocks;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpStateChgSyncJob : IHostedService, IDisposable
    {
        private readonly IErpStateChgNotifierRepository _stateChgNotifierRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpStateChgSyncJob> _logger;

        public ErpStateChgSyncJob(
            IErpStateChgNotifierRepository stateChgNotifierRepository,
            IStockRepository stockRepository,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ErpStateChgSyncJob> logger)
        {
            _stateChgNotifierRepository = stateChgNotifierRepository;
            _stockRepository = stockRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                int sleepTime = 60000;
                while (true)
                {
                    await Task.Delay(sleepTime).ConfigureAwait(false);

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {
                            var notifiers = await _stateChgNotifierRepository.GetAllUnReceivedNotifiersAsync(cancellationToken);
                            if (notifiers == null || notifiers.Count == 0)
                            {
                                //await Task.Delay(sleepTime);
                                continue;
                            }

                            //提取领料通知单号
                            foreach(var notifier in notifiers)
                            {
                                var stocks = await _stockRepository.GetByCheckNoAsync(notifier.PRDT_PH, true).ConfigureAwait(false);
                                if(stocks == null || stocks.Count == 0)
                                {
                                    _logger.Info($"检验单号为{notifier.PRDT_PH}的库存不存在，无法修改库存状态");
                                    notifier.SetInfo("库存不存在");
                                    //notifier.SetIsReceived();
                                }
                                else
                                {
                                    if (notifier.CK_ID == "01")
                                        stocks = stocks.Where(o => o.Warehouse.AreaName == "正常区").ToList();
                                    else if (notifier.CK_ID == "04")
                                        stocks = stocks.Where(o => o.Warehouse.AreaName == "待处理区").ToList();
                                    else
                                    {
                                        _logger.Info($"单号为{notifier.CKZTCHANG_ID}的状态变更通知的库位区域{notifier.CK_ID}无法识别，无法修改库存状态");
                                        notifier.SetInfo("库位区域无法识别");
                                        //notifier.SetIsReceived();
                                    }

                                    if(stocks.Count == 0)
                                    {
                                        _logger.Info($"检验单号为{notifier.PRDT_PH}，库位区域为{notifier.CK_ID}的库存不存在，无法修改库存状态");
                                        notifier.SetInfo("库存不存在");
                                    }
                                    else
                                    {
                                        foreach(var stock in stocks)
                                        {
                                            if (notifier.NEWKCZT_STATE == 0)
                                            {
                                                stock.ReturnToAvailable();
                                            }
                                            else if(notifier.NEWKCZT_STATE == 1)
                                            {
                                                stock.FreezeStock();
                                            }
                                        }

                                        notifier.SetIsReceived();
                                        notifier.SetInfo("变更成功");

                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                    }
                                }
                            }
                            

                            await uow.CompleteAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message);
                            await uow.RollbackAsync().ConfigureAwait(false);
                        }
                    }
                }

            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

