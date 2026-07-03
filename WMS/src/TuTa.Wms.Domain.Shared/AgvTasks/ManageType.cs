using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.AgvTasks
{
    public enum ManageType
    {
        /// <summary>
        /// CTU入库
        /// </summary>
        CTUStockIn,
        /// <summary>
        /// CTU出库
        /// </summary>
        CTUStockOut,
        /// <summary>
        /// 叉车入库
        /// </summary>
        LiftStockIn,
        /// <summary>
        /// 叉车出库
        /// </summary>
        LiftStockOut,
        /// <summary>
        /// agv移动料车
        /// </summary>
        SkipMove,
        /// <summary>
        /// CTU调拨
        /// </summary>
        CTUStockMove,
        /// <summary>
        /// 叉车调拨
        /// </summary>
        LiftStockMove,
        /// <summary>
        /// 物料入库
        /// </summary>
        StockIn,
        /// <summary>
        /// 料车发送
        /// </summary>
        SkipSend,
        /// <summary>
        /// 料车叫回
        /// </summary>
        SkipCall,
        /// <summary>
        /// CTU输送线入库
        /// </summary>
        CTUSSXIn,
        /// <summary>
        /// CTU输送线出库
        /// </summary>
        CTUSSXOut,
        /// <summary>
        /// 叉车输送线入库
        /// </summary>
        LiftSSXIn,
        /// <summary>
        /// 叉车输送线出库
        /// </summary>
        LiftSSXOut,
    }
}
