namespace TuTa.Wms.Cells.Dtos
{
    public class PagedCellsAreaDto
    {
        /// <summary>
        /// 所属库区名
        /// </summary>
        public int areaId { get; set; }

        public bool isHeigh {  get; set; }

        public bool isWeight { get; set; }

        public string cellType { get; set; }

        public string cellCode { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
