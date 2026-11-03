using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities;

/// <summary>从U8同步的来料检验明细；检验单ID与来源明细ID共同唯一，不保存PDA人工检验结论。</summary>
public class ErpMaterialInspection : Entity<Guid>
{
    private ErpMaterialInspection() { }

    /// <summary>建立本地记录；同一U8检验单的不同来源明细分别保存。</summary>
    /// <param name="id">本地记录标识。</param>
    /// <param name="checkId">U8检验单主键，必须为正数。</param>
    /// <param name="sourceAutoId">U8来源明细主键，必须为正数。</param>
    public ErpMaterialInspection(Guid id, long checkId, long sourceAutoId) : base(id)
    {
        if (checkId <= 0 || sourceAutoId <= 0) throw new UserFriendlyException("同步检验单缺少有效的检验单ID或来源明细ID");
        CheckId = checkId;
        SourceAutoId = sourceAutoId;
    }

    /// <summary>U8检验单主键，与来源明细ID共同去重。</summary>
    public long CheckId { get; private set; }
    /// <summary>U8来源到货明细主键，同一检验单下不同来源分别保存。</summary>
    public long SourceAutoId { get; private set; }
    /// <summary>来料检验单号CCHECKCODE。</summary>
    public string CheckCode { get; private set; }
    /// <summary>来源到货单号SOURCECODE，U8未提供时为空。</summary>
    public string SourceCode { get; private set; }
    /// <summary>物料编码CINVCODE，用于PDA精确查询。</summary>
    public string MaterialCode { get; private set; }
    /// <summary>原始批号CBATCH，U8未提供时为空。</summary>
    public string BatchCode { get; private set; }
    /// <summary>合格数量FREGQUANTITY，单位同U8；未提供时为空。</summary>
    public decimal? QualifiedQuantity { get; private set; }
    /// <summary>不合格数量FDISQUANTITY，单位同U8；未提供时为空。</summary>
    public decimal? DisqualifiedQuantity { get; private set; }
    /// <summary>审核人CVERIFIER，未审核时为空。</summary>
    public string Verifier { get; private set; }
    /// <summary>U8是否已审核，独立于PDA人工确认结果。</summary>
    public bool IsVerified { get; private set; }
    /// <summary>U8拒收标志，未提供时为空。</summary>
    public bool? IsRejected { get; private set; }
    /// <summary>对应检验单来源明细是否已全部采购入库成功；仅由本地采购成功记录置为true，U8同步不得重置。</summary>
    public bool IsPurchaseCompleted { get; private set; }
    /// <summary>本地最近写入或更新内容的UTC时间；内容一致时不改写。</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>应用U8当前内容；完全一致时返回false，不更改本地采购完成标记。</summary>
    /// <param name="checkCode">检验单号，必填，最长100字符。</param>
    /// <param name="sourceCode">来源到货单号，可为空，最长100字符。</param>
    /// <param name="materialCode">物料编码，必填，最长100字符。</param>
    /// <param name="batchCode">原始批号，可为空，最长100字符。</param>
    /// <param name="qualifiedQuantity">合格数量，可为空。</param>
    /// <param name="disqualifiedQuantity">不合格数量，可为空。</param>
    /// <param name="verifier">审核人，可为空，最长100字符。</param>
    /// <param name="isVerified">U8审核标志。</param>
    /// <param name="isRejected">U8拒收标志，可为空。</param>
    /// <param name="updatedAt">本次内容更新时间，使用UTC。</param>
    /// <returns>内容是否发生变化。</returns>
    public bool UpdateFromU8(string checkCode, string sourceCode, string materialCode, string batchCode,
        decimal? qualifiedQuantity, decimal? disqualifiedQuantity, string verifier, bool isVerified,
        bool? isRejected, DateTime updatedAt)
    {
        Check.NotNullOrWhiteSpace(checkCode, nameof(checkCode), 100);
        Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode), 100);
        Check.Length(sourceCode, nameof(sourceCode), 100);
        Check.Length(batchCode, nameof(batchCode), 100);
        Check.Length(verifier, nameof(verifier), 100);
        if (CheckCode == checkCode && SourceCode == sourceCode && MaterialCode == materialCode && BatchCode == batchCode &&
            QualifiedQuantity == qualifiedQuantity && DisqualifiedQuantity == disqualifiedQuantity && Verifier == verifier &&
            IsVerified == isVerified && IsRejected == isRejected) return false;
        CheckCode = checkCode;
        SourceCode = sourceCode;
        MaterialCode = materialCode;
        BatchCode = batchCode;
        QualifiedQuantity = qualifiedQuantity;
        DisqualifiedQuantity = disqualifiedQuantity;
        Verifier = verifier;
        IsVerified = isVerified;
        IsRejected = isRejected;
        UpdatedAt = updatedAt;
        return true;
    }
}
