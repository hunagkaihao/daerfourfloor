using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// 生成到货单推送请求DTO
    /// </summary>
    public class PuArrVouchAddRequestDto
    {
        /// <summary>
        /// UID（推送时作为 uid 字段；不传时由系统自动生成）
        /// </summary>
        public long? Uid { get; set; }

        /// <summary>
        /// ASN单号（入库单号 ccode）
        /// </summary>
        [Required]
        public string CAsnCode { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        [Required]
        public string CVenCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        public string Cpocode { get; set; }

        /// <summary>
        /// 明细列表
        /// </summary>
        [Required]
        public List<PuArrVouchDetailRequestDto> Data { get; set; }

        /// <summary>ASN采购部门编码。</summary>
        public string DepartmentCode { get; set; }
        /// <summary>制单人；未提供时使用WMS。</summary>
        public string Maker { get; set; }
        /// <summary>采购类型编码。</summary>
        public string ProcessTypeCode { get; set; }
        /// <summary>币种名称。</summary>
        public string ExchangeName { get; set; }
        /// <summary>税率，百分比；到货接口只接受Int32范围的整数值，保留decimal接收ASN精度，发送前校验。</summary>
        public decimal TaxRate { get; set; }
        /// <summary>汇率；当前U8到货接口只接受Int32范围的整数值，保留decimal源值，真实小数汇率需核对接口契约。</summary>
        public decimal ExchangeRate { get; set; }
    }

    /// <summary>
    /// 生成到货单明细请求DTO
    /// </summary>
    public class PuArrVouchDetailRequestDto
    {
        /// <summary>
        /// 物料料号
        /// </summary>
        [Required]
        public string CInvCode { get; set; }

        /// <summary>
        /// 入库数量；向U8发送时须为Int32范围内的整数，保留decimal源值。
        /// </summary>
        [Required]
        public decimal IQuantity { get; set; }

        /// <summary>
        /// 入库箱数；沿用Math.Round(..., 0)取整规则，结果按Int32发送。
        /// </summary>
        [Required]
        public decimal INum { get; set; }

        /// <summary>
        /// 实际入库数量；向U8发送时须为Int32范围内的整数，保留decimal源值。
        /// </summary>
        [Required]
        public decimal FRealQuantity { get; set; }

        /// <summary>
        /// 实际入库箱数；向U8发送时须为Int32范围内的整数，保留decimal源值。
        /// </summary>
        [Required]
        public decimal FRealNumy { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        [Required]
        public string CBatch { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        public string Cordercode { get; set; }

        /// <summary>
        /// 采购订单行ID（推送ERP时使用，不传则使用默认值）
        /// </summary>
        public long? IPoDetailId { get; set; }

        /// <summary>
        /// 标贴
        /// </summary>
        public string? CFree2 { get; set; }

        /// <summary>
        /// 包装箱
        /// </summary>
        public string? CFree3 { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string? CFree5 { get; set; }

        /// <summary>实际U8仓库编码，不使用接口示例中的仓库。</summary>
        public string WarehouseCode { get; set; }
        /// <summary>是否需要来料检验，1为需要。</summary>
        public int Bgsp { get; set; }
    }

    /// <summary>
    /// 生成到货单推送响应DTO
    /// </summary>
    public class PuArrVouchAddResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ERP返回数据
        /// </summary>
        public object ErpData { get; set; }
    }
}
