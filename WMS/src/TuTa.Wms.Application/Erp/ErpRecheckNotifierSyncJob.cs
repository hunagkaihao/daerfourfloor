using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.PickLists.Aggregates;
using TuTa.Wms.PickLists;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.ValueObjects;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpRecheckNotifierSyncJob : IHostedService, IDisposable
    {
        private readonly IErpRecheckNotifierRepository _erpRecheckNotifierRepository;
        private readonly IRecheckListRepository _recheckListRepository;
        private readonly RecheckListManager _recheckListManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpRecheckNotifierSyncJob> _logger;

        public ErpRecheckNotifierSyncJob(
            IErpRecheckNotifierRepository erpRecheckNotifierRepository,
            IRecheckListRepository recheckListRepository,
            RecheckListManager recheckListManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ErpRecheckNotifierSyncJob> logger)
        {
            _erpRecheckNotifierRepository = erpRecheckNotifierRepository;
            _recheckListRepository = recheckListRepository;
            _recheckListManager = recheckListManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int sleepTime = 60000;
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(sleepTime).ConfigureAwait(false); //1分钟更新一次

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {
                            var erpRecheckNotifiers = await _erpRecheckNotifierRepository.GetAllUnReceivedRecheckNotifiersAsync(true, cancellationToken);
                            if (erpRecheckNotifiers == null || erpRecheckNotifiers.Count == 0)
                            {
                                continue;
                            }

                            List<string> notifierCodes = new List<string>();
                            foreach (var notifier in erpRecheckNotifiers)
                            {
                                if (!notifierCodes.Contains(notifier.CKFQTZD_ID))
                                    notifierCodes.Add(notifier.CKFQTZD_ID);
                            }

                            foreach (string notifierCode in notifierCodes)
                            {
                                var notifiers = erpRecheckNotifiers
                                    .Where(o => o.CKFQTZD_ID == notifierCode)
                                    .OrderBy(o => o.DHTZD_TXM)
                                    .ToList();   //具有相同复检通知单号的记录

                                int deleteState = 0; //1：全部删  2：个别删  3：不删
                                var delNotifiers = notifiers.Where(o => o.IFDELETE == true).ToList();
                                if (delNotifiers.Count == 0)
                                    deleteState = 3;
                                else if (delNotifiers.Count == notifiers.Count)
                                    deleteState = 1;
                                else
                                    deleteState = 2;

                                if (deleteState == 3) //不删，则为增加或修改
                                {
                                    var recheckList = await _recheckListRepository.FindByReCheckListCodeAsync(notifierCode).ConfigureAwait(false);
                                    if (recheckList == null) //本地没有该复检单，全部添加
                                    {
                                        recheckList = await _recheckListManager.CreateReCheckListAsync(notifierCode, notifiers[0].CKFQTZD_DATE).ConfigureAwait(false);
                                        foreach (var notifier in notifiers)
                                        {
                                            await _recheckListManager.AddRecheckItemAsync(
                                                recheckList,
                                                notifier.PRDT_PH,
                                                notifier.DHTZD_TXM,
                                                new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                notifier.FQCQ_NUM,
                                                notifier.FQXH,
                                                notifier.PRDT_DATE);

                                            //冻结
                                        }
                                        await _recheckListRepository.InsertAsync(recheckList);
                                        
                                        foreach (var notifier in notifiers)
                                        {
                                            notifier.SetIsReceived();
                                            await _erpRecheckNotifierRepository.UpdateAsync(notifier);
                                        }
                                    }
                                    else //已经存在该复检单
                                    {
                                        foreach (var notifier in notifiers)
                                        {
                                            var recheckItem = recheckList.GetReCheckItemByBarcode(notifier.PRDT_PH);
                                            if (recheckItem == null)
                                                await _recheckListManager.AddRecheckItemAsync(
                                                    recheckList,
                                                    notifier.PRDT_PH,
                                                    notifier.DHTZD_TXM,
                                                    new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                    notifier.FQCQ_NUM,
                                                    //notifier.FQCQ_NUM,
                                                    notifier.FQXH,
                                                    notifier.PRDT_DATE);
                                            else
                                            {
                                                if (recheckItem.Status == RecheckItemStatus.Created)
                                                    recheckList.ModifyRecheckItem(
                                                        notifier.DHTZD_TXM,
                                                        notifier.PRDT_PH,
                                                        new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                        notifier.FQCQ_NUM,
                                                        //notifier.FQCQ_NUM,
                                                        notifier.FQXH,
                                                        notifier.PRDT_DATE);

                                            }
                                            //冻结
                                        }
                                        await _recheckListRepository.UpdateAsync(recheckList);
                                        
                                        foreach (var notifier in notifiers)
                                        {
                                            notifier.SetIsReceived();
                                            await _erpRecheckNotifierRepository.UpdateAsync(notifier);
                                        }
                                    }
                                }
                                else if (deleteState == 1) //全删
                                {
                                    var recheckList = await _recheckListRepository.FindByReCheckListCodeAsync(notifierCode).ConfigureAwait(false);
                                    if (recheckList != null) //本地没有该复检单，不需要删
                                    {
                                        foreach (var notifier in notifiers) //删除复检项
                                        {
                                            var recheckItem = recheckList.GetReCheckItemByBarcode(notifier.PRDT_PH);
                                            if (recheckItem != null && recheckItem.Status == RecheckItemStatus.Created)
                                                recheckList.RemoveRecheckItem(notifier.PRDT_PH);

                                            //解除冻结
                                        }
                                        if (recheckList.RecheckItems.Count == 0)  //若已没有复检项，则删除复检单
                                            await _recheckListRepository.DeleteAsync(recheckList).ConfigureAwait(false);
                                        else
                                            await _recheckListRepository.UpdateAsync(recheckList).ConfigureAwait(false);
                                    }

                                    foreach (var notifier in notifiers)
                                    {
                                        notifier.SetIsReceived();
                                        await _erpRecheckNotifierRepository.UpdateAsync(notifier);
                                    }
                                }
                                else  //部分删
                                {
                                    var recheckList = await _recheckListRepository.FindByReCheckListCodeAsync(notifierCode).ConfigureAwait(false);
                                    if (recheckList == null) //本地没有该复检单，添加
                                    {
                                        recheckList = await _recheckListManager.CreateReCheckListAsync(notifierCode, notifiers[0].CKFQTZD_DATE).ConfigureAwait(false);
                                        foreach (var notifier in notifiers)
                                        {
                                            if (notifier.IFDELETE != true) //不删的添加
                                                await _recheckListManager.AddRecheckItemAsync(
                                                    recheckList,
                                                    notifier.PRDT_PH,
                                                    notifier.DHTZD_TXM,
                                                    new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                    notifier.FQCQ_NUM,
                                                    //notifier.FQCQ_NUM,
                                                    notifier.FQXH,
                                                    notifier.PRDT_DATE);
                                        }
                                        await _recheckListRepository.InsertAsync(recheckList);

                                        foreach (var notifier in notifiers)
                                        {
                                            notifier.SetIsReceived();
                                            await _erpRecheckNotifierRepository.UpdateAsync(notifier);
                                        }
                                    }
                                    else //已经存在该复检单
                                    {
                                        foreach (var notifier in notifiers)
                                        {
                                            var recheckItem = recheckList.GetReCheckItemByBarcode(notifier.PRDT_PH);
                                            if (recheckItem == null)
                                            {
                                                if (notifier.IFDELETE != true)
                                                    await _recheckListManager.AddRecheckItemAsync(
                                                        recheckList,
                                                        notifier.PRDT_PH,
                                                        notifier.DHTZD_TXM,
                                                        new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                        notifier.FQCQ_NUM,
                                                        //notifier.FQCQ_NUM,
                                                        notifier.FQXH,
                                                        notifier.PRDT_DATE);
                                            }
                                            else
                                            {
                                                if (recheckItem.Status == RecheckItemStatus.Created)
                                                {
                                                    if (notifier.IFDELETE == true)
                                                        recheckList.RemoveRecheckItem(notifier.PRDT_PH);
                                                    else
                                                        recheckList.ModifyRecheckItem(
                                                            notifier.DHTZD_TXM,
                                                            notifier.PRDT_PH,
                                                            new MaterialInfoOfRechkList(notifier.PRDT_ID, notifier.PRDT_NAME, notifier.PRDT_SPEC, notifier.PRDT_UNIT, notifier.PRDT_STOREDAYS),
                                                            notifier.FQCQ_NUM,
                                                            //notifier.FQCQ_NUM,
                                                            notifier.FQXH,
                                                            notifier.PRDT_DATE);
                                                }
                                            }
                                        }
                                        await _recheckListRepository.UpdateAsync(recheckList);

                                        foreach (var notifier in notifiers)
                                        {
                                            notifier.SetIsReceived();
                                            await _erpRecheckNotifierRepository.UpdateAsync(notifier);
                                        }
                                    }
                                }

                                await uow.SaveChangesAsync();

                                await Task.Delay(5).ConfigureAwait(false);
                            }

                            await uow.CompleteAsync();
                            //await Task.Delay(60000).ConfigureAwait(false); //1分钟更新一次
                        }
                        catch (Exception ex)
                        {
                            await uow.RollbackAsync();
                            _logger.Error(ex.Message);
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

