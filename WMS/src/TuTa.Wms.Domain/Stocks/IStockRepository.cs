using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.Cells;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Domain;
using TuTa.Wms.Stocks.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Stocks
{
    public interface IStockRepository : IRepository<Stock, Guid>
    {
        Task<List<Stock>> GetByCellIdAsync(Guid cellId, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<List<Stock>> GetByBoxIdAsync(Guid boxId, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<List<Stock>> GetByBarcodeAsync(string barcode, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<List<Stock>> GetByMaterialCodeAsync(string materialCode, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<List<Stock>> GetByCheckNoAsync(string checkNo, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<Stock> FindByBoxIdAndBarcodeAsync(Guid boxId, string barcode, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<Stock> FindByBoxCodeAndBarcodeAsync(string boxCode, string barcode, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<Stock> FindEarliestStockWithMaterialCodeAsync(string materialCode, bool isTrack = true, CancellationToken cancellationToken = default);

        Task<QueryDataInPage<Stock>> GetPagedStocksAsync(
            string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
            string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
            string receivingMaterialBarcode,
            StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
            decimal? fullRateStart,decimal? fullRateEnd,int cellType,string finGoods,
            EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);

        Task<List<Stock>> GetStocksAsync(
            string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
            string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
            string receivingMaterialBarcode,
            StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
            decimal? fullRateStart, decimal? fullRateEnd, int cellType, string finGoods,
            EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);

        //Task<List<Stock>> GetStocksAsync(
        //    string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
        //    string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
        //    StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
        //    EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
        //    bool isTrack = true,
        //    CancellationToken cancellationToken = default);

        Task<QueryDataInPage<Stock>> GetPagedMoveStocksAsync(
            int areaId,
            string materialName, string checkNo,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);

        Task<QueryDataInPage<Stock>> GetCtuInStocksAsync(
            List<string> cells,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);


        Task<List<Stock>> GetSkipCellStockAsync(
            List<string> cells,
            bool isTrack = true,
            CancellationToken cancellationToken = default);

        Task<Stock> FindByReceivingMaterialBarcodeAsync(string receivingMaterialBarcode, bool isTrack = true, CancellationToken cancellationToken = default);
    }
}
