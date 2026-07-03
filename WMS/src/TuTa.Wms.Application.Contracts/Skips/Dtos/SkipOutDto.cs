using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Skips.Dtos
{
    public class SkipOutDto
    {
        //料车编号
        public string SkipCode { get; set; }
        //料车名称
        public string SkipName { get; set; }
        //点位编号
        public string SkipCellCode { get; set; }
        //料箱数
        public int BindCellCounts { get; set; }
        //运行状态
        public string SkipRunStatus { get; set; }
        //目标站点
        public string TargetCellType { get; set; }
    }
}
