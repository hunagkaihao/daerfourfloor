using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Stocks
{
    public enum RunStatus
    {
        /// <summary>
        /// 待入
        /// </summary>
        In = 0,
        /// <summary>
        /// 库存
        /// </summary>
        Enable = 1,
        /// <summary>
        /// 出库
        /// </summary>
        Out = 2
    }
}
