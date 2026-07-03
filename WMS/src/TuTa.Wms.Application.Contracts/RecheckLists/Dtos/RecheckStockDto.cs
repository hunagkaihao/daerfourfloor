namespace TuTa.Wms.RecheckLists.Dtos
{
    public class RecheckStockDto
    {
        /// <summary>
        /// 针对的复检单号
        /// </summary>
        public string RecheckListCode { get; set; }

        /// <summary>
        /// 复检物料的收料条形码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 复检物料的物料码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 复检物料的物料名
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 复检物料的物料规格
        /// </summary>
        public string MaterialSpecs { get; set; }

        /// <summary>
        /// 复检物料的单位
        /// </summary>
        public string MaterialUnit { get; set; }

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
        /// 物料原检验编号
        /// </summary>
        public string OldCheckNo { get; set; }

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
    }
}
