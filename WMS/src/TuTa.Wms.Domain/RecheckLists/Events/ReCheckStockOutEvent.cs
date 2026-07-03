using System;

using TuTa.Wms.Stocks.Aggregates;

namespace TuTa.Wms.RecheckLists.Events
{
    public class ReCheckStockOutEvent
    {
        /// <summary>
        /// 被出库的库存Id
        /// </summary>
        public Guid StockId { get; set; }

        /// <summary>
        /// 被出库收料码（用于检验）
        /// </summary>
        public string Barcode { get; set; }

        public Stock Stock { get; set; }

        /// <summary>
        /// 领取的数量
        /// </summary>
        public decimal PickedCount { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public string OperatorName { get; set; }
    }
}
