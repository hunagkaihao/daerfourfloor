using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Domain;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Wms.EntityFrameworkCore;

namespace TuTa.Wms.Repositories.Stocks
{
    public class EfCoreStockRepository : EfCoreRepository<WmsDbContext, Stock, Guid>, IStockRepository
    {
        public EfCoreStockRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<Stock>> GetByBoxIdAsync(
            Guid boxId, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.BoxData.BoxId == boxId)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Stock>> GetByCellIdAsync(
            Guid cellId, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.CellData.CellId == cellId)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Stock>> GetByBarcodeAsync(
            string barcode, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.Barcode == barcode)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Stock>> GetByMaterialCodeAsync(
            string materialCode, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.Material.MaterialCode == materialCode)
                .OrderBy(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Stock>> GetByCheckNoAsync(
            string checkNo, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .Where(o => o.CheckData.CheckNo == checkNo)
                .OrderByDescending(o => o.CreationTime)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Stock> FindEarliestStockWithMaterialCodeAsync(
            string materialCode, 
            bool isTrack = true, 
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            var stocks = await dbSet
                .TrackIf(isTrack)
                .Where(o => o.Material.MaterialCode == materialCode)
                .OrderBy(o => o.CheckData.CheckDate ?? DateTime.Today)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return (stocks != null && stocks.Count > 0) ? stocks[0] : null;
        }

        public async Task<Stock> FindByBoxIdAndBarcodeAsync(
            Guid boxId, 
            string barcode, 
            bool isTrack = true, 
            CancellationToken cancelToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.BoxData.BoxId == boxId && o.Barcode == barcode, cancelToken)
                .ConfigureAwait(false);
        }

        public async Task<Stock> FindByBoxCodeAndBarcodeAsync(
            string boxCode,
            string barcode,
            bool isTrack = true,
            CancellationToken cancelToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.BoxData.BoxCode == boxCode && o.Barcode == barcode, cancelToken)
                .ConfigureAwait(false);
        }

        public async Task<QueryDataInPage<Stock>> GetPagedStocksAsync(
            string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
            string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
            string receivingMaterialBarcode,
            StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
            decimal? fullRateStart, decimal? fullRateEnd,int cellType,string finGoods,
            EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Stock> querable = dbSet.TrackIf(isTrack)
                .Where(o => 
                (boxCode == null ? true : o.BoxData.BoxCode == boxCode) &&
                (cellCode == null ? true : o.CellData.CellCode == cellCode) &&
                (warehouseAreaName == null ? true : o.Warehouse.AreaName == warehouseAreaName)&&
                (warehouseName == null ? true : o.Warehouse.HouseName == warehouseName) &&
                (materialCode == null ? true : o.Material.MaterialCode.StartsWith(materialCode)) &&
                (materialNameTip == null ? true : o.Material.MaterialName.Contains(materialNameTip)) &&
                (materialSpecsTip == null ? true : o.Material.Specs.Contains(materialSpecsTip)) &&
                (barcode == null ? true : o.Barcode == barcode) &&
                (receivingMaterialBarcode == null ? true : o.ReceivingMaterialBarcode == receivingMaterialBarcode) &&
                (status == null ? true : o.Status == status) &&
                (stockInType == null ? true : o.StockInType == stockInType) &&
                (stockInDateStart == null || stockInDateEnd == null ? true : o.StockInDate >= stockInDateStart && o.StockInDate <= stockInDateEnd) &&
                (checkType == null ? true : o.CheckData.CheckType == checkType) &&
                (checkResult == null ? true : o.CheckData.CheckResult == checkResult) &&
                (checkNoTip == null ? true : o.CheckData.CheckNo.Contains(checkNoTip)) &&
                (fullRateStart == null ? true :o.BoxData.FullRate >= fullRateStart) &&
                (fullRateEnd == null ? true : o.BoxData.FullRate <= fullRateEnd) &&
                (cellType == 0 ? true : o.CellData.CellType == (CellType)CellType.ToObject(typeof(CellType), cellType)) &&
                (finGoods == null ? true : o.Material.FinGoodsList.Contains(finGoods))
                );

            return new QueryDataInPage<Stock>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    //.OrderByDescending(o => o.CreationTime)
                    .OrderBy(o => o.BoxData.BoxCode)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<List<Stock>> GetStocksAsync(
        string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
            string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
            string receivingMaterialBarcode,
            StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
            decimal? fullRateStart, decimal? fullRateEnd, int cellType, string finGoods,
            EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            List<Stock> stocks = dbSet.TrackIf(isTrack)
                .Where(o =>
                (boxCode == null ? true : o.BoxData.BoxCode == boxCode) &&
                (cellCode == null ? true : o.CellData.CellCode == cellCode) &&
                (warehouseAreaName == null ? true : o.Warehouse.AreaName == warehouseAreaName) &&
                (warehouseName == null ? true : o.Warehouse.HouseName == warehouseName) &&
                (materialCode == null ? true : o.Material.MaterialCode.StartsWith(materialCode)) &&
                (materialNameTip == null ? true : o.Material.MaterialName.Contains(materialNameTip)) &&
                (materialSpecsTip == null ? true : o.Material.Specs.Contains(materialSpecsTip)) &&
                (barcode == null ? true : o.Barcode == barcode) &&
                (receivingMaterialBarcode == null ? true : o.ReceivingMaterialBarcode == receivingMaterialBarcode) &&
                (status == null ? true : o.Status == status) &&
                (stockInType == null ? true : o.StockInType == stockInType) &&
                (stockInDateStart == null || stockInDateEnd == null ? true : o.StockInDate >= stockInDateStart && o.StockInDate <= stockInDateEnd) &&
                (checkType == null ? true : o.CheckData.CheckType == checkType) &&
                (checkResult == null ? true : o.CheckData.CheckResult == checkResult) &&
                (checkNoTip == null ? true : o.CheckData.CheckNo.Contains(checkNoTip)) &&
                (fullRateStart == null ? true : o.BoxData.FullRate >= fullRateStart) &&
                (fullRateEnd == null ? true : o.BoxData.FullRate <= fullRateEnd) &&
                (cellType == 0 ? true : o.CellData.CellType == (CellType)CellType.ToObject(typeof(CellType), cellType)) &&
                (finGoods == null ? true : o.Material.FinGoodsList.Contains(finGoods))
                ).ToList();
            return stocks;
        }



        //public async Task<List<Stock>> GetStocksAsync(
        //    string boxCode, string cellCode, string warehouseAreaName, string warehouseName,
        //    string materialCode, string materialNameTip, string materialSpecsTip, string barcode,
        //    StockStatus? status, StockInType? stockInType, DateTime? stockInDateStart, DateTime? stockInDateEnd,
        //    EnumCheckType? checkType, EnumCheckResult? checkResult, string checkNoTip,
        //    bool isTrack = true, CancellationToken cancellationToken = default)
        //{
        //    var dbSet = await GetDbSetAsync().ConfigureAwait(false);
        //    List<Stock> stocks = await dbSet
        //        .TrackIf(isTrack)
        //        .Where(o =>
        //        (boxCode == null ? true : o.BoxData.BoxCode == boxCode) &&
        //        (cellCode == null ? true : o.CellData.CellCode == cellCode) &&
        //        (warehouseAreaName == null ? true : o.Warehouse.AreaName == warehouseAreaName) &&
        //        (warehouseName == null ? true : o.Warehouse.HouseName == warehouseName) &&
        //        (materialCode == null ? true : o.Material.MaterialCode == materialCode) &&
        //        (materialNameTip == null ? true : o.Material.MaterialName.Contains(materialNameTip)) &&
        //        (materialSpecsTip == null ? true : o.Material.Specs.Contains(materialSpecsTip)) &&
        //        (barcode == null ? true : o.Barcode == barcode) &&
        //        (status == null ? true : o.Status == status) &&
        //        (stockInType == null ? true : o.StockInType == stockInType) &&
        //        (stockInDateStart == null || stockInDateEnd == null ? true : o.StockInDate >= stockInDateStart && o.StockInDate <= stockInDateEnd) &&
        //        (checkType == null ? true : o.CheckData.CheckType == checkType) &&
        //        (checkResult == null ? true : o.CheckData.CheckResult == checkResult) &&
        //        (checkNoTip == null ? true : o.CheckData.CheckNo.Contains(checkNoTip)))
        //        .OrderByDescending(o => o.CreationTime)
        //        .ToListAsync(cancellationToken)
        //        .ConfigureAwait(false);

        //    return stocks;
        //}

        public async Task<QueryDataInPage<Stock>> GetPagedMoveStocksAsync(
            int areaId,
            string materialName, string checkNo,
            bool isTrack = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Stock> querable = dbSet.TrackIf(isTrack)
                .Where(o =>
                (o.Warehouse.AreaId == areaId) &&
                (materialName == null ? true : o.Material.MaterialCode.Contains(materialName)) &&
                (checkNo == null ? true : o.CheckData.CheckNo.Contains(checkNo)));

            return new QueryDataInPage<Stock>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<QueryDataInPage<Stock>> GetCtuInStocksAsync(
            List<string> cellCodes,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            IQueryable<Stock> querable = dbSet.TrackIf(true)
                .Where(o =>
                cellCodes.Contains(o.CellData.CellCode) && o.RunStatus == RunStatus.In);

            return new QueryDataInPage<Stock>()
            {
                TotalCount = await querable.CountAsync(),
                Items = await querable
                    .OrderByDescending(o => o.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        public async Task<List<Stock>> GetSkipCellStockAsync(
            List<string> cellCodes,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return dbSet.TrackIf(isTrack)
                .Where(o =>
                cellCodes.Contains(o.CellData.CellCode)).ToList();
        }

        public async Task<Stock> FindByCellCodeAndMaterialCodeAsync(string cellCode, string materialCode)
        {
            var query = await GetQueryableAsync();
            return await query
                .Include(s => s.Material)
                .FirstOrDefaultAsync(s =>
                    s.CellData.CellCode == cellCode
                    && s.Barcode == materialCode
                    && s.InspectionStatus == InspectionStatus.InProgressInspection);
        }

        public async Task<Stock> FindByReceivingMaterialBarcodeAsync(
            string receivingMaterialBarcode,
            bool isTrack = true,
            CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync().ConfigureAwait(false);
            return await dbSet
                .TrackIf(isTrack)
                .FirstOrDefaultAsync(o => o.ReceivingMaterialBarcode == receivingMaterialBarcode, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
