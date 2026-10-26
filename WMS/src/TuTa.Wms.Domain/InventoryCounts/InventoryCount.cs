using System;
using System.Linq;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.InventoryCounts;

/// <summary>PDA实物盘点明细；每次提交独立保存，以提交标识去重，数量单位为只。</summary>
public class InventoryCount : Entity<Guid>
{
    /// <summary>单个输入数量上限，所有输入均为整数。</summary>
    public const int MaxInputQuantity = 999999999;
    /// <summary>总数最多15位，确保浏览器计算及Excel数字单元格保持精确。</summary>
    public const long MaxTotalQuantity = 999999999999999;

    private InventoryCount() { }

    /// <summary>保存原始条码并解析物料、等级及产商；总数由服务端计算，允许零库存。</summary>
    /// <param name="id">本次提交的稳定标识，网络重试时沿用。</param>
    /// <param name="barcode">纯物料编码或逗号分隔条码；仅第1段物料编码必填，其他段可缺省。</param>
    /// <param name="quantityPerBox">每箱只数，可为空；填写时为1至999999999。</param>
    /// <param name="actualBoxes">实际整箱数，可为空；填写时为0至999999999。</param>
    /// <param name="remainderQuantity">实际零头只数，可为空；填写时为0至999999999。</param>
    /// <param name="recordedAt">服务器本地保存时间，作为按日导出的归属日期。</param>
    public static InventoryCount Create(Guid id, string barcode, int? quantityPerBox,
        int? actualBoxes, int? remainderQuantity, DateTime recordedAt)
    {
        if (id == Guid.Empty) throw new UserFriendlyException("盘点提交标识不能为空");
        var raw = barcode?.Trim();
        if (string.IsNullOrEmpty(raw) || raw.Length > 500)
            throw new UserFriendlyException("请扫描或输入物料码，长度不能超过500个字符");
        var fields = raw.Split(',').Select(x => x.Trim()).ToArray();
        if (string.IsNullOrWhiteSpace(fields[0]))
            throw new UserFriendlyException("物料编码不能为空");
        var manufacturer = Regex.Match(fields.ElementAtOrDefault(1) ?? "", @"^\d{6}([A-Za-z][A-Za-z0-9]*)$");
        var manufacturerCode = manufacturer.Success ? manufacturer.Groups[1].Value : null;
        var grade = fields.ElementAtOrDefault(3);
        if (string.IsNullOrWhiteSpace(grade)) grade = null;
        if (fields[0].Length > 50 || grade?.Length > 20 || manufacturerCode?.Length > 50)
            throw new UserFriendlyException("物料编码或产商代号不能超过50个字符，等级不能超过20个字符");
        if (quantityPerBox < 1 || quantityPerBox > MaxInputQuantity || actualBoxes < 0 ||
            actualBoxes > MaxInputQuantity || remainderQuantity < 0 || remainderQuantity > MaxInputQuantity)
            throw new UserFriendlyException("整箱数量须为正整数，实际箱数和零头须为非负整数，单项最多999999999");
        // 任一数量未填时总数也留空，不能把未盘点误记为实盘0只。
        long? total = quantityPerBox.HasValue && actualBoxes.HasValue && remainderQuantity.HasValue
            ? (long)quantityPerBox.Value * actualBoxes.Value + remainderQuantity.Value : null;
        if (total > MaxTotalQuantity) throw new UserFriendlyException("实际总数不能超过999999999999999只");
        return new InventoryCount {
            Id = id, Barcode = raw, MaterialCode = fields[0], ManufacturerCode = manufacturerCode,
            Grade = grade, QuantityPerBox = quantityPerBox, ActualBoxes = actualBoxes,
            RemainderQuantity = remainderQuantity, TotalQuantity = total, RecordedAt = recordedAt
        };
    }

    /// <summary>扫码原文，用于追溯原始标识卡。</summary>
    public string Barcode { get; private set; }
    /// <summary>物料编码，来自条码第1段。</summary>
    public string MaterialCode { get; private set; }
    /// <summary>产商代号，来自条码第2段去掉六位日期，如F1042；缺失或未识别时为空。</summary>
    public string ManufacturerCode { get; private set; }
    /// <summary>等级，来自条码第4段，如Z3；未提供时为空。</summary>
    public string Grade { get; private set; }
    /// <summary>整箱数量，单位只/箱，可为空；填写时为正整数。</summary>
    public int? QuantityPerBox { get; private set; }
    /// <summary>实际整箱数量，单位箱，可为空；填写时为非负整数。</summary>
    public int? ActualBoxes { get; private set; }
    /// <summary>实际零头数量，单位只，可为空；填写时为非负整数。</summary>
    public int? RemainderQuantity { get; private set; }
    /// <summary>实际总数（只）=整箱数量×实际箱数+实际零头；三项数量未齐时为空。</summary>
    public long? TotalQuantity { get; private set; }
    /// <summary>服务器本地保存时间；按此时间的日期导出。</summary>
    public DateTime RecordedAt { get; private set; }
}
