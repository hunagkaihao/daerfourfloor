using System;
using TuTa.Wms.ChkResultLists;

namespace TuTa.Wms.Stocks.Dtos
{
    public class PagedStockQueryDto
    {
        public string BoxCode { get; set; }

        public string CellCode { get; set; }

        public string WarehouseAreaName { get; set; }

        public string WarehouseName { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string Barcode { get; set; }

        public string ReceivingMaterialBarcode { get; set; }

        public StockStatus? Status { get; set; }

        public StockInType? StockInType { get; set; }

        public DateTime? StockInDateStart { get; set; }

        public DateTime? StockInDateEnd { get; set; }

        public EnumCheckType? CheckType { get; set; }

        public EnumCheckResult? CheckResult { get; set; }

        //0全部 1料箱 2托盘 3分拨墙 4手工
        public int wareType { get; set; }

        public string FinGoods {  get; set; }

        public decimal? FullBoxRateStart { get; set; }

        public decimal? FullBOxRateEnd { get; set; }

        public string AvaType {  get; set; }

        public string CheckNo { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;


    }
}
