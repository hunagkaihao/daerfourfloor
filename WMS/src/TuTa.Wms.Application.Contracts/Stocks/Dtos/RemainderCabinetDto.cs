using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Stocks.Dtos
{
    /// <summary>ASN检验合格后的零头入柜请求；人工平库入库，不创建托盘或AGV任务。</summary>
    public class RemainderCabinetEntryDto
    {
        /// <summary>本次入柜的稳定提交标识，重试必须沿用，成功后下一次入柜才生成新标识。</summary>
        public Guid RequestId { get; set; }
        /// <summary>送货单对应的ASN号。</summary>
        [Required]
        public string AsnCode { get; set; }
        /// <summary>明确选择的本地ASN明细ID，用于确定采购订单、批次及剩余数量。</summary>
        public Guid AsnLineId { get; set; }
        /// <summary>柜位编码：A—L柜、01—15列、01—08层，例如A15-08。</summary>
        [Required]
        public string CellCode { get; set; }
        /// <summary>原始标识卡，逗号第1段为物料，第4段为等级；不能用作唯一实物箱码。</summary>
        [Required]
        public string CardBarcode { get; set; }
        /// <summary>实际入柜零头数量，单位同ASN；须大于0且不超过该明细剩余待入库数量。</summary>
        public decimal Quantity { get; set; }
    }

    /// <summary>4楼叉车库零头格位及固定物料信息，清空库存后仍保留物料和等级绑定。</summary>
    public class RemainderCabinetDto
    {
        /// <summary>标准柜位编码。</summary>
        public string CellCode { get; set; }
        /// <summary>所属仓库名称。</summary>
        public string WarehouseName { get; set; }
        /// <summary>格位是否启用且未设置满货。</summary>
        public bool CanStore { get; set; }
        /// <summary>固定物料编码；尚未首次绑定时为空。</summary>
        public string MaterialCode { get; set; }
        /// <summary>固定物料等级；尚未首次绑定时为空。</summary>
        public string Grade { get; set; }
        /// <summary>同格位各批次及ASN来源的实际库存，数量来自Stocks。</summary>
        public List<RemainderCabinetStockDto> Stocks { get; set; } = new List<RemainderCabinetStockDto>();
    }

    /// <summary>零头柜内的一笔库存，同料同级的不同批次、来源分别记录。</summary>
    public class RemainderCabinetStockDto
    {
        /// <summary>统一库存表Stocks的库存ID。</summary>
        public Guid StockId { get; set; }
        /// <summary>来源ASN号。</summary>
        public string AsnCode { get; set; }
        /// <summary>来源采购订单号。</summary>
        public string OrderCode { get; set; }
        /// <summary>物料编码。</summary>
        public string MaterialCode { get; set; }
        /// <summary>物料名称。</summary>
        public string MaterialName { get; set; }
        /// <summary>真实来源批号。</summary>
        public string BatchCode { get; set; }
        /// <summary>等级。</summary>
        public string Grade { get; set; }
        /// <summary>当前在库数量，单位同库存物料主数据。</summary>
        public decimal Quantity { get; set; }
        /// <summary>库存可用状态名称；Waiting表示等待U8采购入库完成。</summary>
        public string Status { get; set; }
        /// <summary>实际零头柜位编码。</summary>
        public string CellCode { get; set; }
        /// <summary>实际库存计量单位。</summary>
        public string Unit { get; set; }
        /// <summary>是否允许领用：可用、检验合格、未被U8发货或其他业务占用。</summary>
        public bool CanWithdraw { get; set; }
    }
}
