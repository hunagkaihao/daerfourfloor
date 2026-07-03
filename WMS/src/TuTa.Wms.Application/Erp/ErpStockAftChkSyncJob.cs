using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.ValueObjects;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.ValueObjects;

using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpStockAftChkSyncJob : IHostedService, IDisposable
    {
        private readonly IErpStockAftChkRepository _erpStockAftChkRepository;
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly IStockRepository _stockRepository;
        private readonly ChkResultListManager _chkResultListManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpStockAftChkSyncJob> _logger;

        public ErpStockAftChkSyncJob(
            IErpStockAftChkRepository erpStockAftChkRepository,
            IChkResultListRepository inBoundListRepository,
            IBarcodeListRepository barcodeListRepository,
            IStockRepository stockRepository,
            ChkResultListManager inBoundListManager,
            UnitOfWorkManager unitOfWorkManager,
            ILogger<ErpStockAftChkSyncJob> logger)
        {
            _erpStockAftChkRepository = erpStockAftChkRepository;
            _chkResultListRepository = inBoundListRepository;
            _chkResultListManager = inBoundListManager;
            _stockRepository = stockRepository;
            _barcodeListRepository = barcodeListRepository;
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
                    //await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {
                            var erpStocksAftChk = await _erpStockAftChkRepository.GetAllUnReceivedStocksAsync(true, cancellationToken);
                            if (erpStocksAftChk == null || erpStocksAftChk.Count == 0)
                            {
                                await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间更新一次
                                continue;
                            }

                            foreach (var erpStock in erpStocksAftChk)
                            {
                                //StockInTypeHelper.StockInTypeCheck(erpStock.RK_TYPE, "RK_TYPE");
                                CheckTypeHelper.CheckTypeCheck(erpStock.QC_TYPE, "QC_TYPE");
                                CheckResultHelper.CheckResultCheck(erpStock.QCJL, "QCJL");

                                var barcode = await _barcodeListRepository.FindByBarcodeAsync(erpStock.DHTZD_TXM);
                                if (barcode == null)
                                    continue;

                                if (erpStock.IFDELETE != true) //不删，则增加或修改
                                {
                                    var chkResultList = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(erpStock.DHTZD_TXM, (EnumCheckType)erpStock.QC_TYPE).ConfigureAwait(false);
                                    if (chkResultList == null) //本地没有该检验后数据，添加
                                    {
                                        chkResultList = await _chkResultListManager.CreateChkResultListAsync(
                                            erpStock.DHTZD_TXM,
                                            new MaterialInfoOfChkRsltList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                            new CountInfoOfChkRsltList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                            new CheckInfoOfChkRsltList(erpStock.BYQC_ID, erpStock.BYQC_DATE.GetValueOrDefault(), erpStock.QCPRDT_PH, (EnumCheckType)erpStock.QC_TYPE, (EnumCheckResult)erpStock.QCJL, erpStock.QCPASS_NUM, erpStock.OLDQCPRDT_PH),
                                            new SupplierInfoOfChkRsltList(erpStock.GYS_ID, erpStock.GYS_NAME),
                                            new WarehouseInfoOfChkRsltList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                            barcode.StockInType,
                                            erpStock.SCAP_ID,
                                            erpStock.OPBLD_ID,
                                            erpStock.OPBHD_ID).ConfigureAwait(false);
                                        await _chkResultListRepository.InsertAsync(chkResultList);
                                    }
                                    else //已经存在该入库单，修改
                                    {
                                        if (chkResultList.Status == ChkResultListStatus.Create) //只有在入库前才能修改
                                        {
                                            chkResultList.ModifyChkResultList(
                                                new MaterialInfoOfChkRsltList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                                new CountInfoOfChkRsltList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                                new CheckInfoOfChkRsltList(erpStock.BYQC_ID, erpStock.BYQC_DATE.GetValueOrDefault(), erpStock.QCPRDT_PH, (EnumCheckType)erpStock.QC_TYPE, (EnumCheckResult)erpStock.QCJL, erpStock.QCPASS_NUM, erpStock.OLDQCPRDT_PH),
                                                new SupplierInfoOfChkRsltList(erpStock.GYS_ID, erpStock.GYS_NAME),
                                                new WarehouseInfoOfChkRsltList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                                barcode.StockInType,
                                                erpStock.SCAP_ID,
                                                erpStock.OPBLD_ID,
                                                erpStock.OPBHD_ID);
                                            await _chkResultListRepository.UpdateAsync(chkResultList);
                                        }
                                    }

                                    List<Stock> stocks = await _stockRepository.GetByBarcodeAsync(erpStock.DHTZD_TXM);
                                    foreach (Stock stock in stocks)
                                    {
                                        stock.SetCheck(
                                                new CheckInfoOfStock(erpStock.BYQC_ID, erpStock.BYQC_DATE.GetValueOrDefault(), erpStock.QCPRDT_PH, erpStock.OLDQCPRDT_PH, (EnumCheckType)erpStock.QC_TYPE, (EnumCheckResult)erpStock.QCJL, erpStock.QCPASS_NUM));

                                        //if (stock.Status == StockStatus.Available || stock.Status == StockStatus.Freezing || stock.Status == StockStatus.Filtrate)
                                        //{
                                        //    if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.Pass) //检验结果是合格的，解冻结
                                        //        stock.ReturnToAvailable();
                                        //    else if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.NoPass) //检验结果是不合格的，冻结
                                        //        stock.FreezeStock();
                                        //    else if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.Filter) //检验结果是不合格的，冻结
                                        //        stock.SetStatus(StockStatus.Filtrate);
                                        //}

                                        if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.Pass) //检验结果是合格的，解冻结
                                            stock.ReturnToAvailable();
                                        else if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.NoPass) //检验结果是不合格的，冻结
                                            stock.FreezeStock();
                                        else if ((EnumCheckResult)erpStock.QCJL == EnumCheckResult.Filter) //检验结果是不合格的，冻结
                                            stock.SetStatus(StockStatus.Filtrate);

                                        await _stockRepository.UpdateAsync(stock);
                                    }

                                    erpStock.SetIsReceived();
                                    await _erpStockAftChkRepository.UpdateAsync(erpStock);
                                }
                                else //删
                                {
                                    var chkResultList = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(erpStock.DHTZD_TXM, (EnumCheckType)erpStock.QC_TYPE).ConfigureAwait(false);
                                    if (chkResultList != null) //本地已有该入库单，删除
                                        await _chkResultListRepository.DeleteAsync(chkResultList).ConfigureAwait(false);

                                    erpStock.SetIsReceived();
                                    await _erpStockAftChkRepository.UpdateAsync(erpStock);
                                }

                                await uow.SaveChangesAsync();

                                await Task.Delay(5).ConfigureAwait(false);
                            }

                            /*
                            var erpStocksAftChk = await _erpStockAftChkRepository.GetAllUnReceivedStocksAsync(true, cancellationToken);
                            if (erpStocksAftChk == null || erpStocksAftChk.Count == 0)
                            {
                                await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间更新一次
                                continue;
                            }

                            foreach (var erpStock in erpStocksAftChk)
                            {
                                //StockInTypeHelper.StockInTypeCheck(erpStock.RK_TYPE, "RK_TYPE");
                                CheckTypeHelper.CheckTypeCheck(erpStock.QC_TYPE, "QC_TYPE");
                                CheckResultHelper.CheckResultCheck(erpStock.QCJL, "QCJL");

                                var barcode = await _barcodeListRepository.FindByBarcodeAsync(erpStock.DHTZD_TXM);
                                if (barcode == null)
                                    continue;

                                if (erpStock.IFDELETE != true) //不删，则增加或修改
                                {
                                    var chkResultList = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(erpStock.DHTZD_TXM, (EnumCheckType)erpStock.QC_TYPE).ConfigureAwait(false);
                                    if (chkResultList == null) //本地没有该检验后数据，添加
                                    {
                                        chkResultList = await _chkResultListManager.CreateChkResultListAsync(
                                            erpStock.DHTZD_TXM,
                                            new MaterialInfoOfChkRsltList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                            new CountInfoOfChkRsltList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                            new CheckInfoOfChkRsltList(erpStock.BYQC_ID, erpStock.BYQC_DATE.GetValueOrDefault(), erpStock.QCPRDT_PH, (EnumCheckType)erpStock.QC_TYPE, (EnumCheckResult)erpStock.QCJL, erpStock.QCPASS_NUM, erpStock.OLDQCPRDT_PH),
                                            new SupplierInfoOfChkRsltList(erpStock.GYS_ID, erpStock.GYS_NAME),
                                            new WarehouseInfoOfChkRsltList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                            barcode.StockInType,
                                            erpStock.SCAP_ID,
                                            erpStock.OPBLD_ID,
                                            erpStock.OPBHD_ID).ConfigureAwait(false);
                                        await _chkResultListRepository.InsertAsync(chkResultList);
                                    }
                                    else //已经存在该入库单，修改
                                    {
                                        if (chkResultList.Status == ChkResultListStatus.Create) //只有在入库前才能修改
                                        {
                                            chkResultList.ModifyChkResultList(
                                                new MaterialInfoOfChkRsltList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                                new CountInfoOfChkRsltList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                                new CheckInfoOfChkRsltList(erpStock.BYQC_ID, erpStock.BYQC_DATE.GetValueOrDefault(), erpStock.QCPRDT_PH, (EnumCheckType)erpStock.QC_TYPE, (EnumCheckResult)erpStock.QCJL, erpStock.QCPASS_NUM, erpStock.OLDQCPRDT_PH),
                                                new SupplierInfoOfChkRsltList(erpStock.GYS_ID, erpStock.GYS_NAME),
                                                new WarehouseInfoOfChkRsltList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                                barcode.StockInType,
                                                erpStock.SCAP_ID,
                                                erpStock.OPBLD_ID,
                                                erpStock.OPBHD_ID);
                                            await _chkResultListRepository.UpdateAsync(chkResultList);
                                        }
                                    }

                                    erpStock.SetIsReceived();
                                    await _erpStockAftChkRepository.UpdateAsync(erpStock);
                                }
                                else //删
                                {
                                    var chkResultList = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(erpStock.DHTZD_TXM, (EnumCheckType)erpStock.QC_TYPE).ConfigureAwait(false);
                                    if (chkResultList != null) //本地已有该入库单，删除
                                        await _chkResultListRepository.DeleteAsync(chkResultList).ConfigureAwait(false);

                                    erpStock.SetIsReceived();
                                    await _erpStockAftChkRepository.UpdateAsync(erpStock);
                                }

                                await uow.SaveChangesAsync();

                                await Task.Delay(5).ConfigureAwait(false);
                            }
                            */

                            await uow.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            await uow.RollbackAsync();
                            _logger.Error(ex.Message);
                        }
                    }

                    await Task.Delay(sleepTime).ConfigureAwait(false); //1分钟更新一次
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
