using System;
using System.Collections.Generic;
using TuTa.Wms.Erp.Dto;

namespace TuTa.Wms.Erp.IDto
{
    /// <summary>分页查询U8来料检验单；全部筛选留空时沿用U8默认条件，不填入示例单号或日期。</summary>
    public class U8MaterialInspectionQueryDto
    {
        /// <summary>可选存货编码，对应U8的cInvCode；空值不限制物料。</summary>
        public string MaterialCode { get; set; }
        /// <summary>可选来源到货单号，对应sourcecode；空值不限制来源。</summary>
        public string SourceCode { get; set; }
        /// <summary>可选制单开始日期，对应begMakeTime；按U8本地日期零点查询，包含当天及之后的记录。</summary>
        public DateTime? BeginMakeTime { get; set; }
        /// <summary>查询页码，从1开始，对应既有U8列表协议的pagesize。</summary>
        public int Page { get; set; } = 1;
        /// <summary>每页条数，范围1至100，对应U8的pagecount；一次请求只读取一页。</summary>
        public int PageSize { get; set; } = 100;
    }

    /// <summary>按所选U8检验单确认对应ASN明细；分页条件用于重新核对单据及审核状态。</summary>
    public class U8MaterialInspectionConfirmDto : U8MaterialInspectionQueryDto
    {
        /// <summary>所选U8检验单主键，必须为正数。</summary>
        public long CheckId { get; set; }
        /// <summary>所选U8检验单号，必须与重新查询的记录一致。</summary>
        public string CheckCode { get; set; }
        /// <summary>所选检验记录的来源到货明细主键，必须为正数。</summary>
        public long SourceAutoId { get; set; }
        /// <summary>所选检验记录唯一关联的本地ASN号。</summary>
        public string AsnCode { get; set; }
        /// <summary>操作员确认的检验结论：2合格、3不合格；仅已审核的检验单可确认。</summary>
        public int InspectionStatus { get; set; }
    }

    /// <summary>物料来料检验单查询结果；单据存在与检验合格分别展示。</summary>
    public class U8MaterialInspectionResultDto
    {
        /// <summary>当前页请求及解析是否成功；失败不将查询解释为没有单据。</summary>
        public bool Success { get; set; }
        /// <summary>查询说明或失败原因。</summary>
        public string Message { get; set; }
        /// <summary>规范化后的查询物料编码；空值表示不限制物料。</summary>
        public string MaterialCode { get; set; }
        /// <summary>当前页是否存在符合已填条件的来料检验单；HasMore为true时不代表已查全。</summary>
        public bool HasInspection { get; set; }
        /// <summary>本次查询页码，从1开始。</summary>
        public int Page { get; set; }
        /// <summary>请求的每页条数。</summary>
        public int PageSize { get; set; }
        /// <summary>U8声明的记录总数；最终物料及来源仍逐条核对。</summary>
        public int TotalCount { get; set; }
        /// <summary>是否还有后续页，由操作员继续加载，不在同一次请求中自动遍历。</summary>
        public bool HasMore { get; set; }
        /// <summary>当前页按物料及可选来源核对后的检验记录。</summary>
        public List<U8MaterialInspectionItemDto> Items { get; set; } = new List<U8MaterialInspectionItemDto>();
    }

    /// <summary>U8返回的一条来料检验记录及能够严格匹配的本地组盘来源。</summary>
    public class U8MaterialInspectionItemDto
    {
        /// <summary>U8检验单主键；未提供时为空。</summary>
        public long? CheckId { get; set; }
        /// <summary>来料检验单号CCHECKCODE。</summary>
        public string CheckCode { get; set; }
        /// <summary>来源到货单号SOURCECODE。</summary>
        public string SourceCode { get; set; }
        /// <summary>来源到货单明细主键SOURCEAUTOID；未提供时为空。</summary>
        public long? SourceAutoId { get; set; }
        /// <summary>实际返回的存货编码CINVCODE。</summary>
        public string MaterialCode { get; set; }
        /// <summary>批次CBATCH，未提供时为空。</summary>
        public string BatchCode { get; set; }
        /// <summary>合格数量FREGQUANTITY，单位同U8；未提供时为空，不能按0推断。</summary>
        public decimal? QualifiedQuantity { get; set; }
        /// <summary>不合格数量FDISQUANTITY，单位同U8；未提供时为空。</summary>
        public decimal? DisqualifiedQuantity { get; set; }
        /// <summary>U8审核人CVERIFIER，空值表示未审核。</summary>
        public string Verifier { get; set; }
        /// <summary>是否存在U8审核人。</summary>
        public bool IsVerified { get; set; }
        /// <summary>拒收标志BREJFLAG；未提供时为空。</summary>
        public bool? IsRejected { get; set; }
        /// <summary>按到货单、来源明细、物料和批次严格关联的唯一ASN；无唯一来源时为空。</summary>
        public string AsnCode { get; set; }
        /// <summary>匹配的本地ASN明细，供下方组盘沿用既有来源校验，不修改本地检验结论。</summary>
        public List<ErpAsnDto> AsnLines { get; set; } = new List<ErpAsnDto>();
    }
}
