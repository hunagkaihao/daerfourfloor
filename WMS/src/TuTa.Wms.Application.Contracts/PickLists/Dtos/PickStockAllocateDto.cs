using System.Collections.Generic;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PickStockAllocateDto
    {
        /// <summary>
        /// 针对的领用单号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 针对的领料项的唯一码
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// 优先选择的库存
        /// </summary>
        public List<PriorityStock> PriorityStocks { get; set; }
    }

    public class PriorityStock
    {
        public string CellCode { get; set; }

        public string CheckNo { get; set; }
    }
}
