using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.ValueObjects;

namespace TuTa.Wms.Stocks
{
    public class StocksManager : WmsDomainService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IMaterialRepository _materialRepository;

        public StocksManager(IStockRepository stockRepository,
            IMaterialRepository materialRepository)
        {
            _stockRepository = stockRepository;
            _materialRepository = materialRepository;
        }


        public async Task<Stock> CreateStockAsync(
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            //CheckInfoOfStock checkInformation,
            SupplierInfoOfStock supplierInformation,
            StockInType stockInType,
            int isTag,
            string batchCode,
            string bLCode,
            string bHCode)
        {
            // 验证是否需要检验状态
            if (isTag != 1 && isTag != 2)
            {
                throw new Exception($"是否需要检验状态错误，状态为{isTag}");
            }

            Stock stock = new Stock(
                GuidGenerator.Create(),
                barcode, 
                totalCountInTime,
                materialInformation, 
                countInformation, 
                //checkInformation, 
                supplierInformation, 
                stockInType, 
                isTag,
                batchCode, 
                bLCode, 
                bHCode);

            var stocksExist = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
            if (stocksExist != null && stocksExist.Count > 0)
            {
                if (stocksExist[0].Material.MaterialCode != stock.Material.MaterialCode)
                {
                    throw new Exception("已有库存中,存在相同的收料条形码，但对应的物料码不一致");
                }
            }

            return stock;
        }

        public async Task<Stock> CreateStockAsync(
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            //CheckInfoOfStock checkInformation,
            SupplierInfoOfStock supplierInformation,
            StockInType stockInType,
            StockStatus status,
            string batchCode,
            string bLCode,
            string bHCode)
        {
            Stock stock = new Stock(
                GuidGenerator.Create(),
                barcode, 
                totalCountInTime,
                materialInformation, 
                countInformation, 
                //checkInformation, 
                supplierInformation, 
                stockInType, 
                status,
                batchCode, 
                bLCode, 
                bHCode);

            var stocksExist = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
            if (stocksExist != null && stocksExist.Count > 0)
            {
                if (stocksExist[0].Material.MaterialCode != stock.Material.MaterialCode)
                {
                    throw new Exception("已有库存中,存在相同的收料条形码，但对应的物料码不一致");
                }
            }

            return stock;
        }

        public async Task<Stock> CreateStockAsync(
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            CheckInfoOfStock checkInformation,
            SupplierInfoOfStock supplierInformation,
            StockInType stockInType,
            StockStatus status,
            string batchCode,
            string bLCode,
            string bHCode)
        {
            Stock stock = new Stock(
                GuidGenerator.Create(),
                barcode,
                totalCountInTime,
                materialInformation,
                countInformation,
                checkInformation,
                supplierInformation,
                stockInType,
                status,
                batchCode,
                bLCode,
                bHCode);

            var stocksExist = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
            if (stocksExist != null && stocksExist.Count > 0)
            {
                if (stocksExist[0].Material.MaterialCode != stock.Material.MaterialCode)
                {
                    throw new Exception("已有库存中,存在相同的收料条形码，但对应的物料码不一致");
                }
            }

            return stock;
        }
        public async Task<Stock> CreateStockAsync(
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            SupplierInfoOfStock supplierInformation,
            CheckInfoOfStock checkInformation,
            StockInType stockInType,
            string batchCode,
            string bLCode,
            string bHCode)
        {
            Stock stock = new Stock(
                GuidGenerator.Create(),
                barcode,
                totalCountInTime,
                materialInformation,
                countInformation,
                supplierInformation,
                checkInformation,
                stockInType,
                batchCode,
                bLCode,
                bHCode);

            var stocksExist = await _stockRepository.GetByBarcodeAsync(barcode).ConfigureAwait(false);
            if (stocksExist != null && stocksExist.Count > 0)
            {
                if (stocksExist[0].Material.MaterialCode != stock.Material.MaterialCode)
                {
                    throw new Exception("已有库存中,存在相同的收料条形码，但对应的物料码不一致");
                }
            }

            return stock;
        }

        public async Task<bool> IsStockInCellAsync(string stockBarcode, Guid cellId)
        {
            List<Stock> stocks = await _stockRepository.GetByCellIdAsync(cellId, false).ConfigureAwait(false);
            if (stocks == null || stocks.Count == 0)
                return false;

            foreach(Stock stock in stocks)
            {
                if (stock.Barcode == stockBarcode)
                    return true;
            }

            return false;
        }

        public async Task<bool> IsStockInBoxAsync(string stockBarcode, Guid boxId)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(boxId, false).ConfigureAwait(false);
            if (stocks == null || stocks.Count == 0)
                return false;

            foreach (Stock stock in stocks)
            {
                if (stock.Barcode == stockBarcode)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 复检后，更新检验信息，根据检验结果决定是否冻结或解冻，并更新入库时间
        /// </summary>
        /// <param name="stockToUpdate"></param>
        /// <param name="checkOrderCode"></param>
        /// <param name="checkDate"></param>
        /// <param name="checkNo"></param>
        /// <param name="checkType"></param>
        /// <param name="checkResult"></param>
        /// <param name="passCnt"></param>
        /// <returns></returns>
        public Stock UpdateCheckDataAftReCheck(
            Stock stockToUpdate,
            string checkOrderCode,
            DateTime checkDate,
            string checkNo,
            EnumCheckType checkType,
            EnumCheckResult checkResult,
            decimal passCnt)
        {
            if (checkType != EnumCheckType.ReCheck)
                throw new Exception("当前检验不是超期复检");

            stockToUpdate.CheckData.ModifyCheckInfo(checkOrderCode, checkDate, checkNo, checkType, checkResult, passCnt);
            stockToUpdate.UpdateStockInDate(DateTime.Now);
            if (checkResult == EnumCheckResult.Pass) //检验结果是合格的，解冻结
                stockToUpdate.ReturnToAvailable();
            else if (checkResult == EnumCheckResult.NoPass) //检验结果是不合格的，冻结
                stockToUpdate.FreezeStock();
            return stockToUpdate;
        }



        public async Task BoxFullRate(Guid id)
        {
            List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(id);
            decimal isFul = 0;
            foreach (Stock stock in stocks)
            {
                Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);
                if (material == null || material.FullBoxCount.GetValueOrDefault() == 0)
                    throw new Exception($"物料没有录入满箱数据，请处理");
                isFul += stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
            }

            foreach (Stock stock in stocks)
            {
                stock.BoxData.FullRate = isFul;
                await _stockRepository.UpdateAsync(stock);
            }
        }

        //public async Task<Goods> CreateGoodsAsync(
        //    string barcode,
        //    GoodsInfo goodsInformation,
        //    CountInfo countInformation,
        //    CheckInfo checkInformation,
        //    SupplierInfo supplierInformation,
        //    WarehouseInfo warehouseInformation,
        //    int stockInType,
        //    string batchCode,
        //    string bLCode,
        //    string bHCode)
        //{
        //    //物料信息验证
        //    var goodsDefines = await _goodsDefineRepository.GetListAsync(
        //        o => o.GoodsCode == goodsInformation.GoodsCode &&
        //        o.GoodsName == goodsInformation.GoodsName &&
        //        o.Specs == goodsInformation.Specs &&
        //        o.Unit == goodsInformation.Unit).ConfigureAwait(false);

        //    if (goodsDefines == null || goodsDefines.Count == 0)
        //        throw new Exception($"物料码为{goodsInformation.GoodsCode}，物料名为{goodsInformation.GoodsName}，规格为{goodsInformation.Specs}，单位为{goodsInformation.Unit}的物料未定义");

        //    //仓库信息验证


        //    return new Goods(
        //        GuidGenerator.Create(),
        //        barcode,
        //        goodsInformation,
        //        countInformation,
        //        checkInformation,
        //        supplierInformation,
        //        warehouseInformation,
        //        stockInType,
        //        batchCode,
        //        bLCode,
        //        bHCode);
        //}
    }
}
