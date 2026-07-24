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
        /// 入库数量
        /// </summary>
        [Required]
        public decimal IQuantity { get; set; }

        /// <summary>
        /// 入库箱数
        /// </summary>
        [Required]
        public decimal INum { get; set; }

        /// <summary>
        /// 实际入库数量
        /// </summary>
        [Required]
        public decimal FRealQuantity { get; set; }

        /// <summary>
        /// 实际入库箱数
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
