using System;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Stocks.Dtos
{
    /// <summary>
    /// 将已有库存整盘搬运到入库区的申请；来源库位和容器用于校验页面选择是否已经过期。
    /// </summary>
    public class InboundReturnTaskCreateDto
    {
        /// <summary>扫描的完整收料条形码，用其中的物料编号核对所选库存。</summary>
        [Required]
        public string ReceivingMaterialBarcode { get; set; }

        /// <summary>人工选中的库存记录ID，必须对应有数量且已绑定库位、容器的现存库存。</summary>
        public Guid StockId { get; set; }

        /// <summary>查询时选中的来源库位编码，库存已离开该库位时拒绝下发。</summary>
        [Required]
        public string SourceCellCode { get; set; }

        /// <summary>查询时选中的来源容器编码；实际任务搬运该容器中的全部库存。</summary>
        [Required]
        public string BoxCode { get; set; }

        /// <summary>人工输入的目标入库库位编码，必须在同一仓库且为空闲入库库位。</summary>
        [Required]
        public string EndCellCode { get; set; }
    }
}
