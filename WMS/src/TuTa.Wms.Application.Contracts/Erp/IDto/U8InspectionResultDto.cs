using System;
using System.Collections.Generic;

namespace TuTa.Wms.Erp.IDto
{
    /// <summary>当前ASN的U8检验结果；无匹配结果不代表检验合格。</summary>
    public class U8InspectionResultsDto
    {
        /// <summary>查询及解析是否成功，业务可入库性由每行CanPurchase判断。</summary>
        public bool Success { get; set; }
        /// <summary>查询说明或失败原因。</summary>
        public string Message { get; set; }
        /// <summary>与本地来源逐行匹配的结果。</summary>
        public List<U8InspectionLineDto> Items { get; set; } = new List<U8InspectionLineDto>();
    }

    /// <summary>按到货单号、来源明细、物料、批次匹配后的检验单。</summary>
    public class U8InspectionLineDto
    {
        /// <summary>WMS ASN明细ID，不作为U8主键发送。</summary>
        public Guid AsnLineId { get; set; }
        /// <summary>存货编码。</summary>
        public string MaterialCode { get; set; }
        /// <summary>批次。</summary>
        public string BatchCode { get; set; }
        /// <summary>U8检验单主键ID。</summary>
        public long? CheckId { get; set; }
        /// <summary>U8检验单号CCHECKCODE。</summary>
        public string CheckCode { get; set; }
        /// <summary>检验单SOURCEAUTOID，即到货单明细ID。</summary>
        public long? SourceAutoId { get; set; }
        /// <summary>检验单合格数量FREGQUANTITY，单位同ASN。</summary>
        public decimal QualifiedQuantity { get; set; }
        /// <summary>当前明细已组盘完成、人工确认合格且U8检验单已审核、整批合格、来源完整时为true，不受其他ASN明细进度影响。</summary>
        public bool CanPurchase { get; set; }
        /// <summary>已成功报检且本地人工确认为合格，可逐箱组盘；与U8采购入库条件分开判断。</summary>
        public bool CanGroup { get; set; }
        /// <summary>本地累计已组盘件数。</summary>
        public decimal GroupedQuantity { get; set; }
        /// <summary>当前ASN行剩余待组盘件数。</summary>
        public decimal PendingGroupingQuantity { get; set; }
        /// <summary>当前ASN行是否已成功推送采购入库。</summary>
        public bool IsPurchasePushed { get; set; }
        /// <summary>等待原因、检验结果或采购推送错误。</summary>
        public string Message { get; set; }
    }
}
