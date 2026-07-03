namespace TuTa.Wms.Skips.Dtos
{
    public class PagedSkipDto
    {
        /// <summary>
        /// 所属库区名
        /// </summary>
        public int areaId { get; set; }

        public string skipStatus { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
