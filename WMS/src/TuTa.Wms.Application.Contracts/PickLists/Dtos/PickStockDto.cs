namespace TuTa.Wms.PickLists.Dtos
{
    public class PickStockDto
    {
        /// <summary>
        /// 针对的领用单号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 针对的领料项的唯一码
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// 针对的物料码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 针对的物料名
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 针对的物料规格
        /// </summary>
        public string MaterialSpecs { get; set; }

        /// <summary>
        /// 到哪个库中去领
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// 到哪个库区中去领
        /// </summary>
        public string WarehouseAreaName { get; set; }

        /// <summary>
        /// 到哪个库位中去领
        /// </summary>
        public string CellCode { get; set; }

        /// <summary>
        /// 到哪个容器中去领
        /// </summary>
        public string BoxCode { get; set; }

        /// <summary>
        /// 领用物料的收料条形码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 检验单号
        /// </summary>
        public string CheckOrderCode { get; set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        public string CheckNo { get; set; }

        /// <summary>
        /// 物料入库日期
        /// </summary>
        public string StockInDate { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public decimal StockCount { get; set; }

        /// <summary>
        /// 需领用数量
        /// </summary>
        public decimal PickCount { get; set; }

        /// <summary>
        /// 已领用数量
        /// </summary>
        public decimal PickedCount { get; set; }
    }
}
