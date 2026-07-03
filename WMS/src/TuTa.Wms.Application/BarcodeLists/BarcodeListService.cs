using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TuTa.Wms.BarcodeLists.Dtos;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Stocks;
using Volo.Abp;
using Wms.LogTool;

namespace TuTa.Wms.BarcodeLists
{
    public class BarcodeListService : WmsAppService, IBarcodeListService
    {
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILogger<BarcodeListService> _logger;

        public BarcodeListService(
            IBarcodeListRepository barcodeListRepository,
            IMaterialRepository materialRepository,
            ILogger<BarcodeListService> logger
            )
        {
            _barcodeListRepository = barcodeListRepository;
            _materialRepository = materialRepository;
            _logger = logger;
        }

        public async Task<BarcodeDto> GetBarcodeAsync(string barcode)
        {
            try
            {
                var barcodeResult = await _barcodeListRepository.FindByBarcodesAsync(barcode).ConfigureAwait(false);
                if (barcodeResult == null || barcodeResult.Count == 0)
                    throw new Exception($"收料码为{barcode}的检验结论信息不存在");

                if (barcodeResult.Count > 1)
                    throw new Exception($"同时出现多个收料码为{barcode}的检验结论");

                var barcodeList = barcodeResult[0];

                if(barcodeList.InBindCount == barcodeList.ReceiveCount.ReceiveTotalCount)
                    throw new Exception($"该收料码{barcode}已全部绑定");

                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeList.Material.MaterialCode);
                if(material == null)
                    throw new Exception($"该物料码{barcodeList.Material.MaterialCode}不存在");

                return new BarcodeDto()
                {
                    Id = barcodeList.Id.ToString(),
                    Barcode = barcodeList.Barcode,
                    StockInType = StockInTypeHelper.StockInTypeToChinese(barcodeList.StockInType),
                    BatchCode = barcodeList.BatchCode,
                    BLCode = barcodeList.BLCode,
                    BHCode = barcodeList.BHCode,
                    MaterialCode = barcodeList.Material.MaterialCode,
                    MaterialName = barcodeList.Material.MaterialName,
                    Specs = barcodeList.Material.Specs,
                    Unit = barcodeList.Material.Unit,
                    ReceiveTotalCount = barcodeList.ReceiveCount.ReceiveTotalCount,
                    SurplusCount = barcodeList.ReceiveCount.ReceiveTotalCount - barcodeList.InBindCount,
                    ReceivePkgOrBoxCount = barcodeList.ReceiveCount.ReceivePkgOrBoxCount,
                    CountInOnePkgOrBox = barcodeList.ReceiveCount.CountInOnePkgOrBox,
                    SupplierCode = barcodeList.Supplier.SupplierCode,
                    SupplierName = barcodeList.Supplier.SupplierName,
                    SupplierBatchCode = barcodeList.Supplier.SupplierBatchCode,
                    TargetWarehouseCode = barcodeList.Warehouse.TargetWarehouseCode,
                    TargetWarehouseName = barcodeList.Warehouse.TargetWarehouseName,
                    Status = barcodeList.Status.ToString(),
                    InBoundedCount = barcodeList.InBoundedCount,
                    InBindCount = barcodeList.InBindCount,
                    InCheckOutCount = barcodeList.isCheckOutCount,
                    MaxCount = material.FullBoxCount.GetValueOrDefault(),
                    SLDate = barcodeList.SLDate.GetValueOrDefault(),
                    PurchaseId = barcodeList.PurchaseId
                };
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<BarcodeDto> GetBarcodePrintAsync(string barcode)
        {
            try
            {
                var barcodeResult = await _barcodeListRepository.FindByBarcodesAsync(barcode).ConfigureAwait(false);
                if (barcodeResult == null || barcodeResult.Count == 0)
                    throw new Exception($"收料码为{barcode}的检验结论信息不存在");

                if (barcodeResult.Count > 1)
                    throw new Exception($"同时出现多个收料码为{barcode}的收料码");

                var barcodeList = barcodeResult[0];

                Material material = await _materialRepository.FindByMaterialCodeAsync(barcodeList.Material.MaterialCode);

                return new BarcodeDto()
                {
                    Id = barcodeList.Id.ToString(),
                    Barcode = barcodeList.Barcode,
                    StockInType = StockInTypeHelper.StockInTypeToChinese(barcodeList.StockInType),
                    BatchCode = barcodeList.BatchCode,
                    BLCode = barcodeList.BLCode,
                    BHCode = barcodeList.BHCode,
                    MaterialCode = barcodeList.Material.MaterialCode,
                    MaterialName = barcodeList.Material.MaterialName,
                    Specs = barcodeList.Material.Specs,
                    Unit = barcodeList.Material.Unit,
                    ReceiveTotalCount = barcodeList.ReceiveCount.ReceiveTotalCount,
                    ReceivePkgOrBoxCount = barcodeList.ReceiveCount.ReceivePkgOrBoxCount,
                    CountInOnePkgOrBox = barcodeList.ReceiveCount.CountInOnePkgOrBox,
                    SupplierCode = barcodeList.Supplier.SupplierCode,
                    SupplierName = barcodeList.Supplier.SupplierName,
                    SupplierBatchCode = barcodeList.Supplier.SupplierBatchCode,
                    TargetWarehouseCode = barcodeList.Warehouse.TargetWarehouseCode,
                    TargetWarehouseName = barcodeList.Warehouse.TargetWarehouseName,
                    Status = barcodeList.Status.ToString(),
                    InBoundedCount = barcodeList.InBoundedCount,
                    InBindCount = barcodeList.InBindCount,
                    InCheckOutCount = barcodeList.isCheckOutCount,
                    MaxCount = material.FullBoxCount.GetValueOrDefault(),
                    SLDate = barcodeList.SLDate.GetValueOrDefault(),
                    PurchaseId = barcodeList.PurchaseId
                };
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
