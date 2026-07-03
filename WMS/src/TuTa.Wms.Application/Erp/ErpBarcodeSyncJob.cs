using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.ValueObjects;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Stocks;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp
{
    public class ErpBarcodeSyncJob:IHostedService,IDisposable
    {
        private readonly IErpBarcodeRepository _erpBarcodeRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly BarcodeListManager _barcodeListManager;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ErpStockAftChkSyncJob> _logger;

        public ErpBarcodeSyncJob(
            UnitOfWorkManager unitOfWorkManager
            , ILogger<ErpStockAftChkSyncJob> logger
            , IErpBarcodeRepository erpBarcodeRepository
            , IBarcodeListRepository barcodeListRepository
            , BarcodeListManager barcodeListManager
            )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _erpBarcodeRepository = erpBarcodeRepository;
            _barcodeListRepository = barcodeListRepository;
            _barcodeListManager = barcodeListManager;
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
                    await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间

                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        try
                        {

                            var erpBarcode = await _erpBarcodeRepository.GetAllUnReceivedStocksAsync(true, cancellationToken);
                            if (erpBarcode == null || erpBarcode.Count == 0)
                            {
                                //await Task.Delay(sleepTime).ConfigureAwait(false); //间隔一定时间更新一次
                                continue;
                            }

                            foreach (var erpStock in erpBarcode)
                            {
                                StockInTypeHelper.StockInTypeCheck(erpStock.RK_TYPE, "RK_TYPE");

                                if (erpStock.IFDELETE != true) //不删，则增加或修改
                                {
                                    var chkResultList = await _barcodeListRepository.FindByBarcodeAsync(erpStock.DHTZD_TXM).ConfigureAwait(false);
                                    if (chkResultList == null) //本地没有该检验后数据，添加
                                    {
                                        chkResultList = await _barcodeListManager.CreateBarcodeListAsync(
                                            erpStock.DHTZD_TXM,
                                            //erpStock.DHTZD_ID,
                                            erpStock.BYORD_ID,
                                            erpStock.DHTZD_DATE,
                                            new SupplierInfoOfBarcodeList(erpStock.GYS_ID, erpStock.GYS_NAME, erpStock.GYSQC_PH),
                                            new MaterialInfoOfBarcodeList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                            new WarehouseInfoOfBarcodeList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                            new CountInfoOfBarcodeList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                            erpStock.IFQC_TAG,
                                            (StockInType)erpStock.RK_TYPE,
                                            erpStock.SCAP_ID,
                                            erpStock.OPBLD_ID,
                                            erpStock.OPBHD_ID,
                                            erpStock.PRDT_MH).ConfigureAwait(false);
                                        await _barcodeListRepository.InsertAsync(chkResultList);
                                    }
                                    else //已经存在该入库单，修改
                                    {
                                        if (chkResultList.Status == ChkResultListStatus.Create) //只有在入库前才能修改
                                        {
                                            chkResultList.ModifyChkResultList(
                                                new MaterialInfoOfBarcodeList(erpStock.PRDT_ID, erpStock.PRDT_NAME, erpStock.PRDT_SPEC, erpStock.PRDT_UNIT),
                                                new CountInfoOfBarcodeList(erpStock.DHTZD_NUM, erpStock.DHTZD_XS, erpStock.DHTZD_DJSHL),
                                                new SupplierInfoOfBarcodeList(erpStock.GYS_ID, erpStock.GYS_NAME,erpStock.GYSQC_PH),
                                                new WarehouseInfoOfBarcodeList(erpStock.CK_ID.Trim(), erpStock.CK_NAME.Trim()),
                                                erpStock.IFQC_TAG,
                                                erpStock.DHTZD_DATE,
                                                (StockInType)erpStock.RK_TYPE,
                                                erpStock.SCAP_ID,
                                                erpStock.OPBLD_ID,
                                                erpStock.OPBHD_ID,
                                                erpStock.PRDT_MH);
                                            await _barcodeListRepository.UpdateAsync(chkResultList);
                                        }
                                    }

                                    erpStock.SetIsReceived();
                                    await _erpBarcodeRepository.UpdateAsync(erpStock);
                                }
                                else //删
                                {
                                    var chkResultList = await _barcodeListRepository.FindByBarcodeAsync(erpStock.DHTZD_TXM).ConfigureAwait(false);
                                    if (chkResultList != null) //本地已有该入库单，删除
                                        await _barcodeListRepository.DeleteAsync(chkResultList).ConfigureAwait(false);

                                    erpStock.SetIsReceived();
                                    await _erpBarcodeRepository.UpdateAsync(erpStock);
                                }

                                await uow.SaveChangesAsync();

                                await Task.Delay(5).ConfigureAwait(false);
                            }
                            
                            await uow.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            await uow.RollbackAsync();
                            _logger.Error(ex.Message);
                        }
                    }

                    //await Task.Delay(sleepTime).ConfigureAwait(false); //1分钟更新一次
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
