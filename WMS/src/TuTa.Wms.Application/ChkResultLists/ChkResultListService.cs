using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes;
using TuTa.Wms.Cells;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.ChkResultLists.Dtos;
using TuTa.Wms.ChkResultLists.ValueObjects;
using TuTa.Wms.PickLists.Dtos;
using TuTa.Wms.RecheckLists;
using TuTa.Wms.RecheckLists.Entities;
using TuTa.Wms.StockOutHistories;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using TuTa.Wms.Warehouses;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.ChkResultLists
{
    public class ChkResultListService : WmsAppService, IChkResultListService
    {
        private IChkResultListRepository _chkResultListRepository;
        private ChkResultListManager _chkResultListManager;
        private IStockOutHistoryRepository _stockOutHistoryRepository;
        private IBarcodeCheckRepository _barcodeCheckRepository;
        private IBarcodeListRepository _barcodeListRepository;
        private IRecheckItemRepository _recheckItemRepository;
        private IStockRepository _stockRepository;
        private ILogger<ChkResultListService> _logger;

        public ChkResultListService(
            IChkResultListRepository chkResultListRepository,
            ChkResultListManager chkResultListManager,
            IStockOutHistoryRepository stockOutHistoryRepository,
            IBarcodeCheckRepository barcodeCheckRepository,
            IBarcodeListRepository barcodeListRepository,
            IRecheckItemRepository recheckItemRepository,
            IStockRepository stockRepository,
            ILogger<ChkResultListService> logger)
        {
            _chkResultListRepository = chkResultListRepository;
            _chkResultListManager = chkResultListManager;
            _stockOutHistoryRepository = stockOutHistoryRepository;
            _barcodeCheckRepository = barcodeCheckRepository;
            _stockRepository = stockRepository;
            _barcodeListRepository = barcodeListRepository;
            _recheckItemRepository = recheckItemRepository;
            _logger = logger;
        }

        public async Task<ResponseDto> CreateChkResultListFromStockoutHistoryAsync(int stockoutHistoryId)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                try
                {
                    var outHistory = await _stockOutHistoryRepository.FindAsync(stockoutHistoryId).ConfigureAwait(false);
                    if (outHistory == null)
                        throw new Exception($"Id为{stockoutHistoryId}的出库历史记录不存在");

                    var chkResultsExist = await _chkResultListRepository.FindByBarcodeAsync(outHistory.Barcode, false, false).ConfigureAwait(false);
                    if (chkResultsExist != null && chkResultsExist.Count > 0)
                        throw new Exception($"Id为{stockoutHistoryId}的出库历史记录对应的收料条形码在检验结论信息中间表中已存在");

                    var chkResultList = await _chkResultListManager.CreateChkResultListAsync(
                        outHistory.Barcode,
                        new MaterialInfoOfChkRsltList(
                            outHistory.MaterialCode,
                            outHistory.MaterialName,
                            outHistory.MaterialSpecs,
                            outHistory.MaterialUnit),
                        new CountInfoOfChkRsltList(outHistory.OutCount, null, null),
                        new CheckInfoOfChkRsltList(
                            null,
                            outHistory.OutTime,
                            null,
                            EnumCheckType.StockInCheck,
                            CheckResultHelper.ChineseToCheckResult("合格"),
                            outHistory.OutCount,
                            null),
                        new SupplierInfoOfChkRsltList(
                            null,
                            null),
                        new WarehouseInfoOfChkRsltList(
                            outHistory.WarehouseCode,
                            outHistory.WarehouseName),
                        StockInType.AdjustStockIn,
                        null,
                        null,
                        null);

                    await _chkResultListRepository.InsertAsync(chkResultList).ConfigureAwait(false);

                    await uow.CompleteAsync().ConfigureAwait(false);

                    return new ResponseDto() { success = true, message = "从出库历史重新创建检验信息成功" };
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task<ChkResultListDto> GetChkResultListByBarcodeAsync(string barcode)
        {
            try
            {
                var chkResultLists = await _chkResultListRepository.FindByBarcodeAsync(barcode).ConfigureAwait(false);
                if (chkResultLists == null || chkResultLists.Count == 0)
                    throw new Exception($"收料码为{barcode}的检验结论信息不存在");

                if (chkResultLists.Count > 1)
                    throw new Exception($"同时出现多个收料码为{barcode}的检验结论");

                var chkResultList = chkResultLists[0];

                return new ChkResultListDto()
                {
                    Id = chkResultList.Id,
                    Barcode = chkResultList.Barcode,
                    StockInType = StockInTypeHelper.StockInTypeToChinese(chkResultList.StockInType),
                    BatchCode = chkResultList.BatchCode,
                    BLCode = chkResultList.BLCode,
                    BHCode = chkResultList.BHCode,
                    MaterialCode = chkResultList.Material.MaterialCode,
                    MaterialName = chkResultList.Material.MaterialName,
                    Specs = chkResultList.Material.Specs,
                    Unit = chkResultList.Material.Unit,
                    ReceiveTotalCount = chkResultList.ReceiveCount.ReceiveTotalCount,
                    ReceivePkgOrBoxCount = chkResultList.ReceiveCount.ReceivePkgOrBoxCount,
                    CountInOnePkgOrBox = chkResultList.ReceiveCount.CountInOnePkgOrBox,
                    CheckOrderCode = chkResultList.CheckData.CheckOrderCode,
                    CheckDate = chkResultList.CheckData.CheckDate.ToString("yyyy-MM-dd"),
                    CheckNo = chkResultList.CheckData.CheckNo,
                    CheckNoBeforeReCheck = chkResultList.CheckData.CheckNoBeforeReCheck,
                    CheckType = CheckTypeHelper.CheckTypeToChinese(chkResultList.CheckData.CheckType),
                    CheckResult = CheckResultHelper.CheckResultToChinese(chkResultList.CheckData.CheckResult),
                    PassCnt = chkResultList.CheckData.PassCnt,
                    SupplierCode = chkResultList.Supplier.SupplierCode,
                    SupplierName = chkResultList.Supplier.SupplierName,
                    TargetWarehouseCode = chkResultList.Warehouse.TargetWarehouseCode,
                    TargetWarehouseName = chkResultList.Warehouse.TargetWarehouseName,
                    Status = chkResultList.Status.ToString(),
                    InBoundedCount = chkResultList.InBoundedCount
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<PagedResultDto<ChkResultListDto>> GetPagedCheckInItemsAsync(PagedCheckItemQueryDto para)
        {
            try
            {
                if (para.PageIndex <= 0)
                    throw new Exception("分页号无效，必须大于0");

                if (para.PageSize <= 0)
                    throw new Exception("每页数量无效，必须大于0");

                List<BarcodeList> barcodeLists = await _barcodeListRepository.GetAllIsCheck();
                List<ChkResultList> chkResultLists = await _chkResultListRepository.FindByBarcodesAsync(barcodeLists.Select(t=>t.Barcode).ToList());

                List<ChkResultListDto> items = new List<ChkResultListDto>();
                foreach (ChkResultList chkResultList in chkResultLists)
                {
                    ChkResultListDto dto = new ChkResultListDto()
                    {
                        Id = chkResultList.Id,
                        Barcode = chkResultList.Barcode,
                        StockInType = StockInTypeHelper.StockInTypeToChinese(chkResultList.StockInType),
                        BatchCode = chkResultList.BatchCode,
                        BLCode = chkResultList.BLCode,
                        BHCode = chkResultList.BHCode,
                        MaterialCode = chkResultList.Material.MaterialCode,
                        MaterialName = chkResultList.Material.MaterialName,
                        Specs = chkResultList.Material.Specs,
                        Unit = chkResultList.Material.Unit,
                        ReceiveTotalCount = chkResultList.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = chkResultList.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = chkResultList.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = chkResultList.CheckData.CheckOrderCode,
                        CheckDate = chkResultList.CheckData.CheckDate.ToString("yyyy-MM-dd"),
                        CheckNo = chkResultList.CheckData.CheckNo,
                        CheckNoBeforeReCheck = chkResultList.CheckData.CheckNoBeforeReCheck,
                        CheckType = CheckTypeHelper.CheckTypeToChinese(chkResultList.CheckData.CheckType),
                        CheckResult = CheckResultHelper.CheckResultToChinese(chkResultList.CheckData.CheckResult),
                        PassCnt = chkResultList.CheckData.PassCnt,
                        SupplierCode = chkResultList.Supplier.SupplierCode,
                        SupplierName = chkResultList.Supplier.SupplierName,
                        TargetWarehouseCode = chkResultList.Warehouse.TargetWarehouseCode,
                        TargetWarehouseName = chkResultList.Warehouse.TargetWarehouseName,
                        Status = chkResultList.Status.ToString(),
                        InBoundedCount = chkResultList.InBoundedCount,
                        CheckOutCount = barcodeLists.FirstOrDefault(t=>t.Barcode==chkResultList.Barcode).isCheckOutCount
                    };
                    items.Add(dto);
                }

                items = items
                    .Where(o =>
                    (para.QueryBy == 1 ? (para.MaterialCode == null ? true : o.MaterialCode.Contains(para.MaterialCode)) : true) &&
                    (para.QueryBy == 2 ? (para.MaterialName == null ? true : o.MaterialName.Contains(para.MaterialName)) : true) &&
                    (para.QueryBy == 3 ? (para.CheckNo == null ? true : o.CheckNo.Contains(para.CheckNo)) : true)).ToList();


                if (para.SkipCount >= items.Count)    //para.SkipCount大于等于0
                    return new PagedResultDto<ChkResultListDto>() { TotalCount = items.Count, Items = new List<ChkResultListDto>() };
                else
                {
                    List<ChkResultListDto> result = items.GetRange(
                        para.SkipCount,
                        items.Count - para.SkipCount >= para.PageSize ? para.PageSize : items.Count - para.SkipCount);

                    return new PagedResultDto<ChkResultListDto> { TotalCount = items.Count, Items = items };
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<GetChkByBarcodeDto> GetCheckByBarcodeAsync(string barcode,string chkNo)
        {
            try
            {
                List<ChkResultList> chkList = null;
                ChkResultList chkResultList = null;
                decimal checkOutCount = 0;
                if (barcode != null)
                {
                    chkList = await _chkResultListRepository.FindByBarcodeAsync(barcode);

                    chkResultList = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);
                    if (chkResultList == null)
                    {
                        chkResultList = chkList.FirstOrDefault();
                        if (chkResultList == null)
                        {
                            throw new Exception($"获取检验单失败");
                        }
                    }

                    BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(barcode);
                    checkOutCount = barcodeList.isCheckOutCount;
                }
                else
                {
                    chkList = await _chkResultListRepository.FindByChkNoAsync(chkNo);

                    chkResultList = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);
                    if (chkResultList == null)
                    {
                        chkResultList = chkList.FirstOrDefault();
                        if (chkResultList == null)
                        {
                            throw new Exception($"获取检验单失败");
                        }
                    }

                    RecheckItem recheck = await _recheckItemRepository.FindByCheckNoAsync(chkNo);
                    if (recheck == null)
                        throw new Exception($"获取复检单失败");

                    checkOutCount = recheck.CheckCount;
                }

                //List<ChkResultList> chkList = await _chkResultListRepository.FindByBarcodeAsync(barcode);
                //ChkResultList chkResultList = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);
                //if (chkResultList == null)
                //{
                //    chkResultList = chkList.FirstOrDefault();
                //    if (chkResultList == null)
                //    {
                //        throw new Exception($"获取检验单失败");
                //    }
                //}

                //BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(barcode);

                ChkResultListDto chkdto = new ChkResultListDto()
                {
                    Id = chkResultList.Id,
                    Barcode = chkResultList.Barcode,
                    StockInType = StockInTypeHelper.StockInTypeToChinese(chkResultList.StockInType),
                    BatchCode = chkResultList.BatchCode,
                    BLCode = chkResultList.BLCode,
                    BHCode = chkResultList.BHCode,
                    MaterialCode = chkResultList.Material.MaterialCode,
                    MaterialName = chkResultList.Material.MaterialName,
                    Specs = chkResultList.Material.Specs,
                    Unit = chkResultList.Material.Unit,
                    ReceiveTotalCount = chkResultList.ReceiveCount.ReceiveTotalCount,
                    ReceivePkgOrBoxCount = chkResultList.ReceiveCount.ReceivePkgOrBoxCount,
                    CountInOnePkgOrBox = chkResultList.ReceiveCount.CountInOnePkgOrBox,
                    CheckOrderCode = chkResultList.CheckData.CheckOrderCode,
                    CheckDate = chkResultList.CheckData.CheckDate.ToString("yyyy-MM-dd"),
                    CheckNo = chkResultList.CheckData.CheckNo,
                    CheckNoBeforeReCheck = chkResultList.CheckData.CheckNoBeforeReCheck,
                    CheckType = CheckTypeHelper.CheckTypeToChinese(chkResultList.CheckData.CheckType),
                    CheckResult = CheckResultHelper.CheckResultToChinese(chkResultList.CheckData.CheckResult),
                    PassCnt = chkResultList.CheckData.PassCnt,
                    SupplierCode = chkResultList.Supplier.SupplierCode,
                    SupplierName = chkResultList.Supplier.SupplierName,
                    TargetWarehouseCode = chkResultList.Warehouse.TargetWarehouseCode,
                    TargetWarehouseName = chkResultList.Warehouse.TargetWarehouseName,
                    Status = chkResultList.Status.ToString(),
                    InBoundedCount = chkResultList.InBoundedCount,
                    CheckOutCount = checkOutCount
                };

                List<Stock> stocks;
                RecheckItem recitem = await _recheckItemRepository.FindByCheckNoAsync(chkdto.CheckNo);
                if (recitem == null)
                {
                    stocks = await _stockRepository.GetByBarcodeAsync(barcode);
                }
                else
                {
                    stocks = await _stockRepository.GetByCheckNoAsync(chkdto.CheckNo);
                }
                stocks = stocks.Where(t => t.Warehouse.AreaId == 1 || t.Warehouse.AreaId == 2 || t.Warehouse.AreaId == 3).ToList();


                List<StockDto> items = new List<StockDto>();
                foreach (var stock in stocks)
                {
                    StockDto item = new StockDto()
                    {
                        Id = stock.Id,
                        Barcode = stock.Barcode,
                        BoxId = stock.BoxData.BoxId,
                        BoxCode = stock.BoxData.BoxCode,
                        BoxName = stock.BoxData.BoxName,
                        CellId = stock.CellData.CellId,
                        CellCode = stock.CellData.CellCode,
                        CellName = stock.CellData.CellName,
                        HouseId = stock.Warehouse.HouseId,
                        HouseCode = stock.Warehouse.HouseCode,
                        HouseName = stock.Warehouse.HouseName,
                        AreaId = stock.Warehouse.AreaId,
                        AreaCode = stock.Warehouse.AreaCode,
                        AreaName = stock.Warehouse.AreaName,
                        TotalCountInTime = stock.TotalCountInTime,
                        Status = stock.Status.ToString(),
                        StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                        BatchCode = stock.BatchCode,
                        BLCode = stock.BLCode,
                        BHCode = stock.BHCode,
                        StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                        MaterialCode = stock.Material.MaterialCode,
                        MaterialName = stock.Material.MaterialName,
                        Specs = stock.Material.Specs,
                        Unit = stock.Material.Unit,
                        ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                        ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                        CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                        CheckOrderCode = stock.CheckData.CheckOrderCode,
                        CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                        CheckNo = stock.CheckData.CheckNo,
                        CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                        CheckType = stock.CheckData.CheckTypeInChs(),
                        CheckResult = stock.CheckData.CheckResultInChs(),
                        PassCnt = stock.CheckData.PassCnt,
                        SupplierCode = stock.Supplier.SupplierCode,
                        SupplierName = stock.Supplier.SupplierName
                    };
                    items.Add(item);
                }


                GetChkByBarcodeDto dto = new GetChkByBarcodeDto();
                dto.ChkResultList = chkdto;
                dto.items = items;
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
