using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TuTa.Wms.Erp;

/// <summary>来料检验单EF仓储，保证定时同步不会覆盖并发产生的采购完成状态。</summary>
public class ErpMaterialInspectionRepository : EfCoreRepository<WmsDbContext, ErpMaterialInspection, Guid>, IErpMaterialInspectionRepository
{
    /// <summary>使用WMS主数据库，不写入ERP数据库。</summary>
    public ErpMaterialInspectionRepository(IDbContextProvider<WmsDbContext> dbContextProvider) : base(dbContextProvider) { }

    /// <inheritdoc />
    public async Task UpdateU8ContentAsync(IEnumerable<ErpMaterialInspection> records, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = await GetDbContextAsync();
        foreach (var record in records)
        {
            context.Update(record);
            // 采购完成标记由独立SQL置为true，即使本实体加载的是旧值，也不能写回false。
            context.Entry(record).Property(row => row.IsPurchaseCompleted).IsModified = false;
        }
    }

    /// <inheritdoc />
    public async Task<int> MarkPurchaseCompletedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        // 使用List实例Contains，避免数组Contains被绑定为ReadOnlySpan重载，导致EF参数表达式求值失败。
        var keys = ids.Distinct().ToList();
        if (keys.Count == 0) return 0;
        var context = await GetDbContextAsync();
        return await context.Set<ErpMaterialInspection>().Where(row => keys.Contains(row.Id) && !row.IsPurchaseCompleted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.IsPurchaseCompleted, true), cancellationToken);
    }
}
