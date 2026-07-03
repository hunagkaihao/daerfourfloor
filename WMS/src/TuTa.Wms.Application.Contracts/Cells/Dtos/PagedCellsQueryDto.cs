namespace TuTa.Wms.Cells.Dtos
{
    public class PagedCellsQueryDto
    {
        public string WarehouseName { get; set; }

        /// <summary>
        /// 所属库区名
        /// </summary>
        public string WarehouseAreaName { get; set; }

        /// <summary>
        /// 所属架子名称
        /// </summary>
        public string ShelfName { get; set; }

        public string CellCodeTip { get; set; }

        public string CellNameTip { get; set; }

        /// <summary>
        /// 库位类型
        /// </summary>
        public string CellType { get; set; }

        /// <summary>
        /// 可存放的容器规格名称，以半角逗号分隔
        /// </summary>
        public string AvailableBoxSpecsNamesTip { get; set; }

        /// <summary>
        /// 库位状态，有货、无货、满货
        /// </summary>
        public string CellStatus { get; private set; }

        /// <summary>
        /// 运行状态，禁用、可用、选定等
        /// </summary>
        public string RunStatus { get; private set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
