using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Skips.Dtos
{
    public class SkipInDto
    {
        public string SkipCode { get; set; }
        public string SkipName { get; set; }
        public string SkipCellCode { get; set; }
        public int BindCellCounts { get; set; }
        public string SkipRunStatus { get; set; }
        public int inSkipStatusCount { get; set; }
    }
}
