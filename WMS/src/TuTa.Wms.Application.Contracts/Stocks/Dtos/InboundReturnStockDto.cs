using System.Collections.Generic;

namespace TuTa.Wms.Stocks.Dtos
{
    /// <summary>回库扫码命中的库存及其所在库位的整托明细，供人工确认混托搬运范围。</summary>
    public class InboundReturnStockDto : StockDto
    {
        /// <summary>当前库位所有数量大于零的库存，包含扫码未命中的其他物料及批次。</summary>
        public List<StockDto> CellStocks { get; set; } = new List<StockDto>();

        /// <summary>当前库位的物料种类数，按物料编号去重，不按批次或库存条数计数。</summary>
        public int CellMaterialCount { get; set; }

        /// <summary>当前库位全部库存的实时数量之和；各物料的计量单位在明细中显示。</summary>
        public decimal CellTotalCount { get; set; }

        /// <summary>当前库位全部库存的实时箱数之和；任一条库存箱数缺失或无效时返回null，不按收料码推算。</summary>
        public long? CellTotalBoxCount { get; set; }
    }
}
