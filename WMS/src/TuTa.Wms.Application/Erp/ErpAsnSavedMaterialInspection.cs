using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp;

public partial class ErpAsnAppService
{
    /// <summary>单进程内防止后台同步轮次重叠，多实例另使用同步分布式锁。</summary>
    private static readonly SemaphoreSlim MaterialInspectionSyncLock = new(1, 1);
    /// <summary>页面查询与定时同步共用的批次写入锁，避免同时插入相同检验明细。</summary>
    private static readonly SemaphoreSlim MaterialInspectionSaveLock = new(1, 1);

    /// <summary>从数据库按物料查询全部未完成采购入库的检验记录，不限制条数、不调用U8。</summary>
    /// <param name="input">物料编码，也支持取逗号分隔箱码的首段。</param>
    /// <returns>该物料尚未完成采购入库的全部检验单及当前可关联的ASN来源。</returns>
    [UnitOfWork(IsDisabled = true)]
    public async Task<U8MaterialInspectionResultDto> GetSavedMaterialInspectionResultsAsync(SavedMaterialInspectionQueryDto input)
    {
        input ??= new SavedMaterialInspectionQueryDto();
        var code = input.MaterialCode?.Split(',')[0].Trim();
        var result = new U8MaterialInspectionResultDto { MaterialCode = code, Page = 1 };
        if (string.IsNullOrWhiteSpace(code))
        {
            result.Message = "请扫描或输入物料码";
            return result;
        }
        try
        {
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                var query = (await _materialInspectionRepository.GetQueryableAsync())
                    .Where(row => row.MaterialCode == code && !row.IsPurchaseCompleted);
                var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(row => row.CheckId).ThenBy(row => row.SourceAutoId));
                result.Items = rows.Select(MapSavedMaterialInspection).ToList();
                result.TotalCount = rows.Count;
                // 复用现有返回结构，单次包含全部记录，不再有后续页。
                result.PageSize = rows.Count;
                await read.CompleteAsync();
            }
            await AttachInspectionAsnSourcesAsync(result.Items);
            result.Success = true;
            result.HasInspection = result.Items.Count > 0;
            result.HasMore = false;
            result.Message = result.HasInspection ? $"本地查询到{result.Items.Count}条来料检验记录" :
                "本地暂无未完成采购入库的来料检验单；已完成记录已跳过，新单据请等待同步";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "本地来料检验单查询失败：物料={MaterialCode}，原因={Reason}", code, ex.Message);
            result.Message = "本地来料检验单查询失败：" + ex.Message;
            return result;
        }
    }

    /// <summary>后台从服务器当天00:00开始读取U8全部分页，不限制总条数；每页独立提交，保留历史数据及人工结论。</summary>
    /// <param name="cancellationToken">宿主停止时中断分页和数据库写入。</param>
    internal async Task SynchronizeMaterialInspectionsAsync(CancellationToken cancellationToken)
    {
        if (!await MaterialInspectionSyncLock.WaitAsync(0, cancellationToken)) return;
        try
        {
            await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:MaterialInspectionSync");
            if (_u8Lock != null && handle == null)
            {
                _logger.LogInformation("其他实例正在同步来料检验单，本轮跳过");
                return;
            }
            // 每轮固定当天零点，跨零点分页时仍使用同一条件，下一轮自动切换到新的当天。
            var beginMakeTime = DateTime.Today;
            _logger.LogInformation("开始同步U8来料检验单到本地数据库：制单开始时间={BeginMakeTime:yyyy-MM-dd HH:mm:ss}，读取全部分页", beginMakeTime);
            var token = await GetErpTokenAsync(Guid.NewGuid().ToString(), cancellationToken);
            if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("U8登录失败");
            using var client = CreateU8Client();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var inserted = 0;
            var updated = 0;
            var unchanged = 0;
            for (var page = 1; ; page++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // 100仅为U8单页条数，持续读取至HasMore为false，不限制本轮同步总量。
                var result = await ReadU8MaterialInspectionPageAsync(new U8MaterialInspectionQueryDto {
                    BeginMakeTime = beginMakeTime, Page = page, PageSize = 100 },
                    client, cancellationToken);
                if (!result.Success) throw new InvalidOperationException($"第{page}页同步失败：{result.Message}");
                var saved = await SaveMaterialInspectionItemsAsync(result.Items, cancellationToken);
                inserted += saved.Inserted;
                updated += saved.Updated;
                unchanged += saved.Unchanged;
                _logger.LogInformation("U8来料检验单同步第{Page}页：U8共{Total}条，本页{Count}条，新增{Inserted}条，更新{Updated}条，一致跳过{Unchanged}条，回填采购完成{Completed}条",
                    page, result.TotalCount, result.Items.Count, saved.Inserted, saved.Updated, saved.Unchanged, saved.Completed);
                if (!result.HasMore) break;
            }
            _logger.LogInformation("U8来料检验单同步完成：新增{Inserted}条，更新{Updated}条，一致跳过{Unchanged}条", inserted, updated, unchanged);
        }
        finally { MaterialInspectionSyncLock.Release(); }
    }

    /// <summary>页面查询与定时同步共用的事务保存入口，新增插入、变化更新、一致跳过，保留本地采购完成标记。</summary>
    /// <param name="items">本次U8查询返回的记录；检验单ID与来源明细ID均必须为正数。</param>
    /// <param name="cancellationToken">后台停止时取消等待或写入。</param>
    /// <returns>新增、更新、一致跳过和回填采购完成的记录数；保存失败抛出异常，整批回滚。</returns>
    private async Task<(int Inserted, int Updated, int Unchanged, int Completed)> SaveMaterialInspectionItemsAsync(
        List<U8MaterialInspectionItemDto> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0) return (0, 0, 0, 0);
        if (!await MaterialInspectionSaveLock.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
            throw new InvalidOperationException("来料检验单正在保存，请稍后重试");
        try
        {
            await using var handle = _u8Lock == null ? null :
                await _u8Lock.TryAcquireAsync("U8:MaterialInspectionSave", TimeSpan.FromSeconds(10), cancellationToken);
            if (_u8Lock != null && handle == null)
                throw new InvalidOperationException("其他实例正在保存来料检验单，请稍后重试");
            using var write = UnitOfWorkManager.Begin(true, true);
            var checkIds = items.Where(item => item.CheckId.HasValue).Select(item => item.CheckId.Value).Distinct().ToList();
            var existing = (await _materialInspectionRepository.GetListAsync(row => checkIds.Contains(row.CheckId), cancellationToken: cancellationToken))
                .ToDictionary(row => (row.CheckId, row.SourceAutoId));
            var addedRows = new List<ErpMaterialInspection>();
            var changedRows = new HashSet<ErpMaterialInspection>();
            var unchanged = 0;
            foreach (var item in items)
            {
                if (!(item.CheckId > 0) || !(item.SourceAutoId > 0))
                    throw new InvalidOperationException($"检验单{item.CheckCode}缺少有效检验单ID或来源明细ID，未保存本批记录");
                var key = (item.CheckId.Value, item.SourceAutoId.Value);
                var isNew = !existing.TryGetValue(key, out var row);
                if (isNew)
                {
                    row = new ErpMaterialInspection(GuidGenerator.Create(), key.Item1, key.Item2);
                    existing.Add(key, row);
                    addedRows.Add(row);
                }
                var changed = row.UpdateFromU8(item.CheckCode, item.SourceCode, item.MaterialCode, item.BatchCode,
                    item.QualifiedQuantity, item.DisqualifiedQuantity, item.Verifier, item.IsVerified, item.IsRejected, DateTime.UtcNow);
                if (!changed) unchanged++;
                else if (!isNew && !addedRows.Contains(row)) changedRows.Add(row);
            }
            if (addedRows.Count > 0) await _materialInspectionRepository.InsertManyAsync(addedRows, cancellationToken: cancellationToken);
            if (changedRows.Count > 0) await _materialInspectionRepository.UpdateU8ContentAsync(changedRows, cancellationToken);
            // 先落库新增记录，再按真实ASN成功状态回填；采购完成标记始终独立更新。
            await write.SaveChangesAsync(cancellationToken);
            var records = items.Select(item => existing[(item.CheckId.Value, item.SourceAutoId.Value)]).Distinct().ToList();
            var completed = await MarkInspectionPurchasesCompletedAsync(records, cancellationToken);
            await write.CompleteAsync(cancellationToken);
            return (addedRows.Count, changedRows.Count, unchanged, completed);
        }
        finally { MaterialInspectionSaveLock.Release(); }
    }

    /// <summary>只依据已保存的ASN采购成功回执标记对应检验明细；同一来源关联多条ASN时须全部成功。</summary>
    /// <param name="records">本次待核对的本地检验记录。</param>
    /// <param name="cancellationToken">宿主停止时取消。</param>
    /// <returns>本次新增的采购完成标记数量；调用方须提供写事务。</returns>
    private async Task<int> MarkInspectionPurchasesCompletedAsync(List<ErpMaterialInspection> records,
        CancellationToken cancellationToken = default)
    {
        records = records.Where(row => !row.IsPurchaseCompleted).ToList();
        if (records.Count == 0) return 0;
        var sources = await _erpAsnRepository.GetInspectionSourcesAsync(records.Select(row => row.MaterialCode).Distinct().ToList());
        var grouped = sources.Where(line => line.VerifiedInspectionId.HasValue && line.ArrivalDetailId.HasValue)
            .ToLookup(line => (line.VerifiedInspectionId.Value, line.ArrivalDetailId.Value));
        var completedIds = new List<Guid>();
        foreach (var record in records)
        {
            var matches = grouped[(record.CheckId, record.SourceAutoId)].Where(line =>
                line.VerifiedInspectionCode?.Trim() == record.CheckCode && line.MaterialCode == record.MaterialCode &&
                line.ArrivalVoucherCode == record.SourceCode && (line.BatchCode ?? "") == (record.BatchCode ?? "")).ToList();
            if (matches.Count > 0 && matches.All(line => line.IsPurchasePushed)) completedIds.Add(record.Id);
        }
        return await _materialInspectionRepository.MarkPurchaseCompletedAsync(completedIds, cancellationToken);
    }

    /// <summary>转换已保存的U8明细；ASN关联在查询时重新计算，避免持久化过期的组盘数量。</summary>
    /// <param name="row">本地保存的检验单明细。</param>
    /// <returns>供现有PDA卡片显示的检验记录。</returns>
    private static U8MaterialInspectionItemDto MapSavedMaterialInspection(ErpMaterialInspection row) => new()
    {
        CheckId = row.CheckId, CheckCode = row.CheckCode, SourceAutoId = row.SourceAutoId,
        SourceCode = row.SourceCode, MaterialCode = row.MaterialCode, BatchCode = row.BatchCode,
        QualifiedQuantity = row.QualifiedQuantity, DisqualifiedQuantity = row.DisqualifiedQuantity,
        Verifier = row.Verifier, IsVerified = row.IsVerified, IsRejected = row.IsRejected
    };
}
