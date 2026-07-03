using System;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PagedCheckItemQueryDto
    {

        /// <summary>
        /// 根据什么查询：1、按照物料号查询，2、按照物料名查询，3、检验编号
        /// </summary>
        public int? QueryBy { get; set; }

        public string MaterialName { get; set; }

        public string MaterialCode { get; set; }

        public string CheckNo { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;


    }
}
