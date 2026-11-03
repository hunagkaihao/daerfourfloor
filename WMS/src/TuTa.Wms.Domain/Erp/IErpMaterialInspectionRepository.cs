using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Erp;

/// <summary>来料检验单仓储；隔离U8字段更新与本地采购完成标记，避免并发覆盖。</summary>
public interface IErpMaterialInspectionRepository : IRepository<ErpMaterialInspection, Guid>
{
    /// <summary>登记已跟踪记录的U8内容修改，排除采购完成标记；由调用方工作单元保存。</summary>
    /// <param name="records">待更新的现有检验记录，不包含新建记录。</param>
    /// <param name="cancellationToken">宿主停止时取消。</param>
    Task UpdateU8ContentAsync(IEnumerable<ErpMaterialInspection> records, CancellationToken cancellationToken = default);

    /// <summary>仅将给定本地记录的采购完成标记原子更新为true，不覆盖U8字段。</summary>
    /// <param name="ids">已核实对应来源全部采购成功的本地检验明细ID。</param>
    /// <param name="cancellationToken">宿主停止时取消。</param>
    /// <returns>本次从未完成改为完成的记录数。</returns>
    Task<int> MarkPurchaseCompletedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
