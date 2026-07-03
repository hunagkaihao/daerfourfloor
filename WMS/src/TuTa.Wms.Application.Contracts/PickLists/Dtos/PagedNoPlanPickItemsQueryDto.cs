using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PagedNoPlanPickItemsQueryDto
    {
        /// <summary>
        /// 领料单所属部门
        /// </summary>
        public Guid? DepartmentId { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string MaterialCodeTip { get; set; }

        public string PickerName { get; set; }

        public int? PickTypeNo { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int SkipCount => (PageIndex - 1) * PageSize;

        public int MaxResultCount => PageSize;
    }
}
