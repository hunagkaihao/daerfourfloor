using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuTa.Wms.AgvTasks.Aggregaes
{
    public class AGVOptions
    {
        /// <summary>
        /// AGV服务器地址
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public string Enable { get; set; }
        /// <summary>
        /// 叉车任务类型
        /// </summary>
        public string LiftTaskType { get; set; }
        /// <summary>
        /// CTU入库任务类型
        /// </summary>
        public string CTUTaskType { get; set; }
        /// <summary>
        /// 货架任务
        /// </summary>
        public string SkipTaskType { get; set; }
        /// <summary>
        /// 货架发送
        /// </summary>
        public string SkipSendType { get; set; }
        /// <summary>
        /// 货架叫回
        /// </summary>
        public string SkipCallType { get; set; }

        /// <summary>
        /// CTU输送线
        /// </summary>
        public string CTUTaskXianType { get; set; }

        /// <summary>
        /// 叉车输送线
        /// </summary>
        public string LiftTaskXianType { get; set; }

        /// <summary>
        /// 容器搬运任务(createStockTask)模板
        /// </summary>
        public string CreateStockTaskType { get; set; }

        /// <summary>
        /// 容器搬运出库任务(createStockTaskV2)模板
        /// </summary>
        public string CreateStockOutTaskType { get; set; }

        /// <summary>
        /// 库存整理专用RCS任务模板。
        /// 该模板只允许库存整理入口使用，不能替换普通入库、出库任务模板。
        /// </summary>
        public string StockConsolidationTaskType { get; set; } = "De03";
    }
}
