using System;
using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.InventoryCounts;

/// <summary>PDA盘点提交，只接收条码及实盘输入，解析字段与总数由服务端确定。</summary>
public class InventoryCountCreateDto
{
    /// <summary>本次提交标识；网络重试沿用，同一标识不能用于不同数据。</summary>
    public Guid RequestId { get; set; }
    /// <summary>纯物料编码或扫码原文，仅第1段物料必填，第2段批次、第4段等级可缺省。</summary>
    [Required, StringLength(500)]
    public string Barcode { get; set; }
    /// <summary>每箱只数，可为空；填写时为1至999999999。</summary>
    [Range(1, 999999999)]
    public int? QuantityPerBox { get; set; }
    /// <summary>实际整箱数，可为空；填写时为0至999999999。</summary>
    [Range(0, 999999999)]
    public int? ActualBoxes { get; set; }
    /// <summary>实际零头只数，可为空；填写时为0至999999999。</summary>
    [Range(0, 999999999)]
    public int? RemainderQuantity { get; set; }
}
