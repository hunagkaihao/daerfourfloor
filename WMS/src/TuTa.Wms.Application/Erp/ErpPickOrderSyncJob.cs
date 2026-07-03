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
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpPickOrderSyncJob : IHostedService, IDisposable
    {
        private readonly IErpPickOrderRepository _erpPickListRepository;
        private readonly IPickListRepository _pickListRepository;
        private readonly PickListManager _pickOrderManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpPickOrderSyncJob> _logger;

        public ErpPickOrderSyncJob(
            IErpPickOrderRepository erpPickOrderRepository,
            IPickListRepository pickOrderRepository,
            PickListManager pickOrderManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ErpPickOrderSyncJob> logger)
        {
            _erpPickListRepository = erpPickOrderRepository;
            _pickListRepository = pickOrderRepository;
            _pickOrderManager = pickOrderManager;
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
                            var erpPickLists = await _erpPickListRepository.GetAllUnReceivedPickOrdersAsync(true, cancellationToken);
                            if (erpPickLists == null || erpPickLists.Count == 0)
                            {
                                //await Task.Delay(sleepTime);
                                continue;
                            }

                            //提取领料通知单号
                            List<string> pickListCodes = new List<string>();
                            foreach (var list in erpPickLists)
                            {
                                if (!pickListCodes.Contains(list.CHKTZD_ID))
                                {
                                    pickListCodes.Add(list.CHKTZD_ID);
                                }
                            }

                            //判断ERP中具有相同领料单号的领料单共同部分是否一致，如果不一致，暂时不同步
                            foreach (var code in pickListCodes)
                            {
                                var erpLists = await _erpPickListRepository.GetPickOrdersWithListCodeAsync(code).ConfigureAwait(false);
                                if (erpLists.Count == 1)
                                    continue;

                                bool isCommonPartSame = true;
                                var firstList = erpLists[0];
                                for (int i = 1; i < erpLists.Count; i++)
                                {
                                    if (firstList.CHKTZD_ID != erpLists[i].CHKTZD_ID ||
                                        firstList.CHKTZD_DATE != erpLists[i].CHKTZD_DATE ||
                                        firstList.CHKTZD_TYPE != erpLists[i].CHKTZD_TYPE ||
                                        firstList.CHKTZD_DEPT != erpLists[i].CHKTZD_DEPT ||
                                        firstList.CHKTZDDEPT_NAME != erpLists[i].CHKTZDDEPT_NAME ||
                                        firstList.CHKTZD_GYS != erpLists[i].CHKTZD_GYS ||
                                        firstList.CHKTZDGYS_NAME != erpLists[i].CHKTZDGYS_NAME ||
                                        firstList.CHKTZPRDT_PH != erpLists[i].CHKTZPRDT_PH ||
                                        firstList.CHKTZDCP_ID != erpLists[i].CHKTZDCP_ID ||
                                        firstList.CHKTZDCP_NAME != erpLists[i].CHKTZDCP_NAME ||
                                        firstList.CHKTZDCP_SPEC != erpLists[i].CHKTZDCP_SPEC)
                                    {
                                        isCommonPartSame = false;
                                        break;
                                    }
                                }

                                if (!isCommonPartSame)
                                {
                                    var listsTemp = erpPickLists.Where(o => o.CHKTZD_ID == code).ToList();
                                    foreach (var list in listsTemp)
                                    {
                                        list.SetInfo("领料单共同部分数据不一致，暂不同步");
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        erpPickLists.Remove(list);
                                    }

                                    _logger.Info($"领料单号为{code}的领料单共同部分数据不一致，暂不同步");
                                }
                            }

                            foreach(var erpList in erpPickLists)
                            {
                                if (erpList.IFDELETE == true)
                                {
                                    var localList = await _pickListRepository.FindByPickListCodeAsync(erpList.CHKTZD_ID).ConfigureAwait(false);
                                    if (localList == null)
                                    {
                                        erpList.SetInfo("Wms没有该领料单，默认删除成功");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP删除 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项，本地没有该领料单，默认删除成功");
                                        continue;
                                    }
                                    var localItem = localList.GetPickItemByUniqueCode(erpList.CHKTZD_ITM);
                                    if (localItem == null)
                                    {
                                        erpList.SetInfo("Wms没有该领料项，默认删除成功");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP删除 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项，本地没有该领料项，默认删除成功");
                                        continue;
                                    }
                                    if (localItem.Status != PickItemStatus.Created)
                                    {
                                        erpList.SetInfo("领料项已经在执行或执行完成，不能删除");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP删除 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项，但该领料项已经在执行或执行完成，不能删除");
                                        continue;
                                    }

                                    localList.RemovePickItem(erpList.CHKTZD_ITM);
                                    if (localList.PickItems.Count == 0)
                                        await _pickListRepository.DeleteAsync(localList).ConfigureAwait(false);

                                    erpList.SetInfo("领料项已成功删除");
                                    erpList.SetIsReceived();
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                    _logger.Info($"ERP删除 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项，成功！");
                                    continue;
                                }
                                else
                                {
                                    var localList = await _pickListRepository.FindByPickListCodeAsync(erpList.CHKTZD_ID).ConfigureAwait(false);
                                    if (localList == null) //增加领料单及领料项
                                    {
                                        PickerInfoOfPickList picker = new PickerInfoOfPickList(
                                            erpList.CHKTZD_DEPT,
                                            erpList.CHKTZDDEPT_NAME,
                                            erpList.CHKTZD_GYS,
                                            erpList.CHKTZDGYS_NAME);

                                        GoodsInfoOfPickList goods = new GoodsInfoOfPickList(
                                            erpList.CHKTZDCP_ID,
                                            erpList.CHKTZDCP_NAME,
                                            erpList.CHKTZDCP_SPEC);

                                        var localListToAdd = await _pickOrderManager.CreatePickList(
                                            erpList.CHKTZD_ID, erpList.CHKTZD_DATE,
                                            erpList.CHKTZD_TYPE, picker,
                                            erpList.CHKTZPRDT_PH, goods)
                                            .ConfigureAwait(false);

                                        await _pickOrderManager.AddPickItem(
                                            localListToAdd,
                                            erpList.CHKTZD_ITM,
                                            erpList.PRDT_ID,
                                            erpList.PRDT_NAME,
                                            erpList.PRDT_SPEC,
                                            erpList.PRDT_UNIT,
                                            erpList.CHKTZD_NUM,
                                            erpList.PRDT_PH);

                                        await _pickListRepository.InsertAsync(localListToAdd).ConfigureAwait(false);

                                        erpList.SetInfo("Wms增加该领料单成功");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP增加 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料单成功");
                                        continue;
                                    }


                                    if (erpList.CHKTZD_ID != localList.PickListCode ||
                                        erpList.CHKTZD_DATE != localList.PickListDate ||
                                        erpList.CHKTZD_TYPE != (int)localList.Type ||
                                        erpList.CHKTZD_DEPT != localList.Picker.DeptCode ||
                                        erpList.CHKTZDDEPT_NAME != localList.Picker.DeptName ||
                                        erpList.CHKTZD_GYS != localList.Picker.GysCode ||
                                        erpList.CHKTZDGYS_NAME != localList.Picker.GysName ||
                                        erpList.CHKTZPRDT_PH != localList.PickBatch ||
                                        erpList.CHKTZDCP_ID != localList.Goods.GoodsCode ||
                                        erpList.CHKTZDCP_NAME != localList.Goods.GoodsName ||
                                        erpList.CHKTZDCP_SPEC != localList.Goods.GoodsSpecs)
                                    {
                                        if (localList.Status != PickOrderStatus.Created)
                                        {
                                            erpList.SetInfo("领料单已经在执行或执行完成，无法修改");
                                            erpList.SetIsReceived();
                                            await uow.SaveChangesAsync().ConfigureAwait(false);
                                            _logger.Info($"单号为{erpList.CHKTZD_ID}的领料单已在执行或执行完成，ERP修改该领料单共同数据失败！");
                                            continue;
                                        }
                                        else
                                        {
                                            localList.ModifyPickList(
                                                erpList.CHKTZD_DATE,
                                                erpList.CHKTZD_TYPE,
                                                new PickerInfoOfPickList(
                                                    erpList.CHKTZD_DEPT,
                                                    erpList.CHKTZDDEPT_NAME,
                                                    erpList.CHKTZD_GYS,
                                                    erpList.CHKTZDGYS_NAME),
                                                erpList.CHKTZPRDT_PH,
                                                new GoodsInfoOfPickList(
                                                    erpList.CHKTZDCP_ID,
                                                    erpList.CHKTZDCP_NAME,
                                                    erpList.CHKTZDCP_SPEC));

                                            erpList.SetInfo("领料单修改成功");
                                            erpList.SetIsReceived();
                                            await uow.SaveChangesAsync().ConfigureAwait(false);
                                            _logger.Info($"ERP修改 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料单，成功！");
                                            //continue;
                                        }
                                    }

                                    var localItem = localList.GetPickItemByUniqueCode(erpList.CHKTZD_ITM);
                                    if (localItem == null)  //增加领料项
                                    {
                                        await _pickOrderManager.AddPickItem(
                                            localList,
                                            erpList.CHKTZD_ITM,
                                            erpList.PRDT_ID,
                                            erpList.PRDT_NAME,
                                            erpList.PRDT_SPEC,
                                            erpList.PRDT_UNIT,
                                            erpList.CHKTZD_NUM,
                                            erpList.PRDT_PH);

                                        await _pickListRepository.UpdateAsync(localList).ConfigureAwait(false);

                                        erpList.SetInfo("Wms增加该领料项成功");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP增加 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项成功");
                                        continue;
                                    }
                                    if (localItem.Status != PickItemStatus.Created)
                                    {
                                        erpList.SetInfo("领料项已经在执行或执行完成，不能修改");
                                        erpList.SetIsReceived();
                                        await uow.SaveChangesAsync().ConfigureAwait(false);
                                        _logger.Info($"ERP修改 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项，但该领料项已经在执行或执行完成，不能修改");
                                        continue;
                                    }

                                    localList.ModifyPickItem(
                                        erpList.CHKTZD_ITM,
                                        erpList.PRDT_ID,
                                        erpList.PRDT_NAME,
                                        erpList.PRDT_SPEC,
                                        erpList.PRDT_UNIT,
                                        erpList.CHKTZD_NUM);

                                    await _pickListRepository.UpdateAsync(localList).ConfigureAwait(false);

                                    erpList.SetInfo("Wms修改该领料单成功");
                                    erpList.SetIsReceived();
                                    await uow.SaveChangesAsync().ConfigureAwait(false);
                                    _logger.Info($"ERP修改 领料单号为{erpList.CHKTZD_ID}，唯一码为{erpList.CHKTZD_ITM}的领料项成功");
                                    continue;
                                }
                            }

                            await uow.CompleteAsync().ConfigureAwait(false);

                            #region old 
                            //处理每个领料单
                            //foreach (var pickListCode in pickListCodes)
                            //{
                            //    var pickListsInErp = erpPickLists.Where(o => o.CHKTZD_ID == pickListCode).OrderBy(o => o.PRDT_ID).ToList();

                            //    int deleteState = 0; //1：全部删  2：个别删  3：不删
                            //    var delLists = pickListsInErp.Where(o => o.IFDELETE == true).ToList();
                            //    if (delLists.Count == 0)
                            //        deleteState = 3;
                            //    else if (delLists.Count == pickListsInErp.Count)
                            //        deleteState = 1;
                            //    else
                            //        deleteState = 2;

                            //    if (deleteState == 3) //不删，则为增加或修改
                            //    {
                            //        var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                            //        if (pickList == null) //本地没有该复检单，全部添加
                            //        {
                            //            PickerInfoOfPickList picker = new PickerInfoOfPickList(
                            //                pickListsInErp[0].CHKTZD_DEPT,
                            //                pickListsInErp[0].CHKTZDDEPT_NAME,
                            //                pickListsInErp[0].CHKTZD_GYS,
                            //                pickListsInErp[0].CHKTZDGYS_NAME);

                            //            GoodsInfoOfPickList goods = new GoodsInfoOfPickList(
                            //                pickListsInErp[0].CHKTZDCP_ID,
                            //                pickListsInErp[0].CHKTZDCP_NAME,
                            //                pickListsInErp[0].CHKTZDCP_SPEC);

                            //            pickList = await _pickOrderManager.CreatePickList(
                            //                pickListCode, pickListsInErp[0].CHKTZD_DATE,
                            //                (PickType)pickListsInErp[0].CHKTZD_TYPE, picker,
                            //                pickListsInErp[0].CHKTZPRDT_PH, goods)
                            //                .ConfigureAwait(false);

                            //            foreach(var pickOrder in pickListsInErp)
                            //            {
                            //                await _pickOrderManager.AddPickItem(
                            //                    pickList,
                            //                    pickOrder.CHKTZD_ITM,
                            //                    pickOrder.PRDT_ID,
                            //                    pickOrder.PRDT_NAME,
                            //                    pickOrder.PRDT_SPEC,
                            //                    pickOrder.PRDT_UNIT,
                            //                    pickOrder.CHKTZD_NUM);
                            //            }

                            //            await _pickListRepository.InsertAsync(pickList);

                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                pickOrder.SetIsReceived();
                            //                await _erpPickListRepository.UpdateAsync(pickOrder);
                            //            }
                            //        }
                            //        else //已经存在该复检单
                            //        {
                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                var pickItem = pickList.GetPickItemByUniqueCode(pickOrder.CHKTZD_ITM);
                            //                if (pickItem == null)
                            //                    await _pickOrderManager.AddPickItem(
                            //                        pickList,
                            //                        pickOrder.CHKTZD_ITM,
                            //                        pickOrder.PRDT_ID,
                            //                        pickOrder.PRDT_NAME,
                            //                        pickOrder.PRDT_SPEC,
                            //                        pickOrder.PRDT_UNIT,
                            //                        pickOrder.CHKTZD_NUM);
                            //                else
                            //                {
                            //                    if (pickItem.Status == PickItemStatus.Created)
                            //                            pickList.ModifyPickItem(
                            //                                pickOrder.CHKTZD_ITM,
                            //                                pickOrder.PRDT_ID,
                            //                                pickOrder.PRDT_NAME,
                            //                                pickOrder.PRDT_SPEC,
                            //                                pickOrder.PRDT_UNIT,
                            //                                pickOrder.CHKTZD_NUM);
                            //                }
                            //            }
                            //            await _pickListRepository.UpdateAsync(pickList);

                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                pickOrder.SetIsReceived();
                            //                await _erpPickListRepository.UpdateAsync(pickOrder);
                            //            }
                            //        }
                            //    }
                            //    else if (deleteState == 1) //全删
                            //    {
                            //        var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                            //        if (pickList != null) //本地没有该复检单，不需要删
                            //        {
                            //            foreach (var pickOrder in pickListsInErp) //删除复检项
                            //            {
                            //                var pickItem = pickList.GetPickItemByUniqueCode(pickOrder.CHKTZD_ITM);
                            //                if (pickItem != null && pickItem.Status == PickItemStatus.Created)
                            //                    pickList.RemovePickItem(pickOrder.CHKTZD_ITM);
                            //            }
                            //            if (pickList.PickItems.Count == 0)  //若已没有复检项，则删除复检单
                            //                await _pickListRepository.DeleteAsync(pickList).ConfigureAwait(false);
                            //            else
                            //                await _pickListRepository.UpdateAsync(pickList).ConfigureAwait(false);
                            //        }

                            //        foreach (var pickOrder in pickListsInErp)
                            //        {
                            //            pickOrder.SetIsReceived();
                            //            await _erpPickListRepository.UpdateAsync(pickOrder);
                            //        }
                            //    }
                            //    else  //部分删
                            //    {
                            //        var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode).ConfigureAwait(false);
                            //        if (pickList == null) //本地没有该复检单，添加
                            //        {
                            //            PickerInfoOfPickList picker = new PickerInfoOfPickList(
                            //                pickListsInErp[0].CHKTZD_DEPT,
                            //                pickListsInErp[0].CHKTZDDEPT_NAME,
                            //                pickListsInErp[0].CHKTZD_GYS,
                            //                pickListsInErp[0].CHKTZDGYS_NAME);

                            //            GoodsInfoOfPickList goods = new GoodsInfoOfPickList(
                            //                pickListsInErp[0].CHKTZDCP_ID,
                            //                pickListsInErp[0].CHKTZDCP_NAME,
                            //                pickListsInErp[0].CHKTZDCP_SPEC);

                            //            pickList = await _pickOrderManager.CreatePickList(
                            //                pickListCode, pickListsInErp[0].CHKTZD_DATE,
                            //                (PickType)pickListsInErp[0].CHKTZD_TYPE, picker,
                            //                pickListsInErp[0].CHKTZPRDT_PH, goods)
                            //                .ConfigureAwait(false);

                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                if (pickOrder.IFDELETE != true) //不删的添加
                            //                    await _pickOrderManager.AddPickItem(
                            //                        pickList,
                            //                        pickOrder.CHKTZD_ITM,
                            //                        pickOrder.PRDT_ID,
                            //                        pickOrder.PRDT_NAME,
                            //                        pickOrder.PRDT_SPEC,
                            //                        pickOrder.PRDT_UNIT,
                            //                        pickOrder.CHKTZD_NUM);
                            //            }
                            //            await _pickListRepository.InsertAsync(pickList);

                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                pickOrder.SetIsReceived();
                            //                await _erpPickListRepository.UpdateAsync(pickOrder);
                            //            }
                            //        }
                            //        else //已经存在该复检单
                            //        {
                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                var pickItem = pickList.GetPickItemByUniqueCode(pickOrder.CHKTZD_ITM);
                            //                if (pickItem == null)
                            //                {
                            //                    if (pickOrder.IFDELETE != true)
                            //                        await _pickOrderManager.AddPickItem(
                            //                            pickList,
                            //                            pickOrder.CHKTZD_ITM,
                            //                            pickOrder.PRDT_ID,
                            //                            pickOrder.PRDT_NAME,
                            //                            pickOrder.PRDT_SPEC,
                            //                            pickOrder.PRDT_UNIT,
                            //                            pickOrder.CHKTZD_NUM);
                            //                }
                            //                else
                            //                {
                            //                    if (pickItem.Status == PickItemStatus.Created)
                            //                    {
                            //                        if (pickOrder.IFDELETE == true)
                            //                            pickList.RemovePickItem(pickOrder.CHKTZD_ITM);
                            //                        else
                            //                            pickList.ModifyPickItem(
                            //                                pickOrder.CHKTZD_ITM,
                            //                                pickOrder.PRDT_ID,
                            //                                pickOrder.PRDT_NAME,
                            //                                pickOrder.PRDT_SPEC,
                            //                                pickOrder.PRDT_UNIT,
                            //                                pickOrder.CHKTZD_NUM);
                            //                    }
                            //                }
                            //            }
                            //            await _pickListRepository.UpdateAsync(pickList);

                            //            foreach (var pickOrder in pickListsInErp)
                            //            {
                            //                pickOrder.SetIsReceived();
                            //                await _erpPickListRepository.UpdateAsync(pickOrder);
                            //            }
                            //        }
                            //    }

                            //    await uow.SaveChangesAsync();

                            //    await Task.Delay(5).ConfigureAwait(false);
                            //}

                            //await uow.CompleteAsync().ConfigureAwait(false);

                            //foreach (var erpPickOrder in erpPickOrders)
                            //{
                            //    if (erpPickOrder.IFDELETE == true) //这个领用单被停用了或删除了，本地的领用单同步删除
                            //    {
                            //        var localPickOrder = await _pickListRepository.FindByPickListCodeAsync(erpPickOrder.CHKTZD_ID).ConfigureAwait(false);
                            //        if (localPickOrder != null) //存在该领料单的本地记录
                            //        {
                            //            var pickItem = localPickOrder.GetPickItemByUniqueCode(erpPickOrder.CHKTZD_ITM);
                            //            if (pickItem != null) //本地领用单中存在这个被停用或删除物料
                            //            {
                            //                if (pickItem.Status == PickItemStatus.Created) //该物料还未开始领料，才能被删除
                            //                {
                            //                    localPickOrder.RemovePickItem(pickItem.UniqueCode);
                            //                    if (localPickOrder.PickItems.Count == 0)
                            //                        await _pickListRepository.DeleteAsync(localPickOrder).ConfigureAwait(false);
                            //                    else
                            //                        await _pickListRepository.UpdateAsync(localPickOrder).ConfigureAwait(false);                                                
                            //                }
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        var localPickList = await _pickListRepository.FindByPickListCodeAsync(erpPickOrder.CHKTZD_ID).ConfigureAwait(false);
                            //        if (localPickList != null) //该领料单已经在本地，说明ERP对物料进行了修改，本地物料需要更新
                            //        {
                            //            if (localPickList.PickListDate != erpPickOrder.CHKTZD_DATE ||
                            //                localPickList.Type != (PickType)erpPickOrder.CHKTZD_TYPE ||
                            //                localPickList.Picker.DeptCode != erpPickOrder.CHKTZD_DEPT ||
                            //                localPickList.Picker.DeptName != erpPickOrder.CHKTZDDEPT_NAME ||
                            //                localPickList.Picker.GysCode != erpPickOrder.CHKTZD_GYS ||
                            //                localPickList.Picker.GysName != erpPickOrder.CHKTZDGYS_NAME ||
                            //                localPickList.PickBatch != erpPickOrder.CHKTZPRDT_PH ||
                            //                localPickList.Goods.GoodsCode != erpPickOrder.CHKTZDCP_ID ||
                            //                localPickList.Goods.GoodsName != erpPickOrder.CHKTZDCP_NAME ||
                            //                localPickList.Goods.GoodsSpecs != erpPickOrder.CHKTZDCP_SPEC)
                            //            {
                            //                localPickList.ModifyPickList(
                            //                    erpPickOrder.CHKTZD_DATE,
                            //                    (PickType)erpPickOrder.CHKTZD_TYPE,
                            //                    new PickerInfoOfPickList(
                            //                        erpPickOrder.CHKTZD_DEPT,
                            //                        erpPickOrder.CHKTZDDEPT_NAME,
                            //                        erpPickOrder.CHKTZD_GYS,
                            //                        erpPickOrder.CHKTZDGYS_NAME),
                            //                    erpPickOrder.CHKTZPRDT_PH,
                            //                    new GoodsInfoOfPickList(
                            //                        erpPickOrder.CHKTZDCP_ID,
                            //                        erpPickOrder.CHKTZDCP_NAME,
                            //                        erpPickOrder.CHKTZDCP_SPEC));
                            //            }

                            //            var item = localPickList.GetPickItemByUniqueCode(erpPickOrder.CHKTZD_ITM);
                            //            if (item == null)
                            //            {
                            //                await _pickOrderManager.AddPickItem(
                            //                    localPickList,
                            //                    erpPickOrder.CHKTZD_ITM,
                            //                    erpPickOrder.PRDT_ID,
                            //                    erpPickOrder.PRDT_NAME,
                            //                    erpPickOrder.PRDT_SPEC,
                            //                    erpPickOrder.PRDT_UNIT,
                            //                    erpPickOrder.CHKTZD_NUM);
                            //            }
                            //            else
                            //            {
                            //                if (item.Status == PickItemStatus.Created)
                            //                {
                            //                    localPickList.ModifyPickItem(
                            //                        erpPickOrder.CHKTZD_ITM,
                            //                        erpPickOrder.PRDT_ID,
                            //                        erpPickOrder.PRDT_NAME,
                            //                        erpPickOrder.PRDT_SPEC,
                            //                        erpPickOrder.PRDT_UNIT,
                            //                        erpPickOrder.CHKTZD_NUM);
                            //                }
                            //            }
                            //            await _pickListRepository.UpdateAsync(localPickList).ConfigureAwait(false);
                            //        }
                            //        else //属于新的领料单，本地需要增加
                            //        {
                            //            PickerInfoOfPickList picker = new PickerInfoOfPickList(
                            //                erpPickOrder.CHKTZD_DEPT,
                            //                erpPickOrder.CHKTZDDEPT_NAME,
                            //                erpPickOrder.CHKTZD_GYS,
                            //                erpPickOrder.CHKTZDGYS_NAME);

                            //            GoodsInfoOfPickList goods = new GoodsInfoOfPickList(
                            //                erpPickOrder.CHKTZDCP_ID,
                            //                erpPickOrder.CHKTZDCP_NAME,
                            //                erpPickOrder.CHKTZDCP_SPEC);

                            //            var newPickList = await _pickOrderManager.CreatePickList(
                            //                erpPickOrder.CHKTZD_ID, erpPickOrder.CHKTZD_DATE,
                            //                (PickType)erpPickOrder.CHKTZD_TYPE, picker,
                            //                erpPickOrder.CHKTZPRDT_PH, goods)
                            //                .ConfigureAwait(false);

                            //            await _pickOrderManager.AddPickItem(
                            //                newPickList,
                            //                erpPickOrder.CHKTZD_ITM,
                            //                erpPickOrder.PRDT_ID,
                            //                erpPickOrder.PRDT_NAME,
                            //                erpPickOrder.PRDT_SPEC,
                            //                erpPickOrder.PRDT_UNIT,
                            //                erpPickOrder.CHKTZD_NUM);

                            //            await _pickListRepository.InsertAsync(newPickList).ConfigureAwait(false);
                            //        }
                            //    }
                            //    await uow.SaveChangesAsync().ConfigureAwait(false);
                            //    await Task.Delay(5);
                            //}

                            //foreach (var erpPickOrder in erpPickOrders)
                            //{
                            //    erpPickOrder.SetIsReceived();
                            //    await _erpPickOrderRepository.UpdateAsync(erpPickOrder).ConfigureAwait(false);
                            //    await Task.Delay(2);
                            //}
                            //await uow.SaveChangesAsync().ConfigureAwait(false);

                            //await uow.CompleteAsync().ConfigureAwait(false);

                            //await Task.Delay(sleepTime).ConfigureAwait(false); //1分钟更新一次
                            #endregion
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
