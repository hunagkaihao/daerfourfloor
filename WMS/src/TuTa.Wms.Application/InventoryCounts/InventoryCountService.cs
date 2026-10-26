using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using TuTa.Wms.Application.Contracts.Shared;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.InventoryCounts;

/// <summary>盘点独立落库，保留每次实盘结果，导出使用现有NPOI生成真正的xlsx文件。</summary>
public class InventoryCountService : WmsAppService, IInventoryCountService
{
    /// <summary>盘点明细仓储。</summary>
    private readonly IRepository<InventoryCount, Guid> _repository;

    /// <summary>注入独立盘点表仓储。</summary>
    public InventoryCountService(IRepository<InventoryCount, Guid> repository) => _repository = repository;

    /// <inheritdoc />
    public async Task<ResponseDto> CreateAsync(InventoryCountCreateDto input)
    {
        if (input == null) throw new UserFriendlyException("请填写盘点数据");
        var record = InventoryCount.Create(input.RequestId, input.Barcode, input.QuantityPerBox,
            input.ActualBoxes, input.RemainderQuantity, DateTime.Now);
        var existing = await _repository.FindAsync(record.Id);
        if (existing != null)
        {
            if (existing.Barcode != record.Barcode || existing.QuantityPerBox != record.QuantityPerBox ||
                existing.ActualBoxes != record.ActualBoxes || existing.RemainderQuantity != record.RemainderQuantity)
                throw new UserFriendlyException("该提交标识已保存其他盘点数据，请核对后重新录入");
            return new ResponseDto { success = true, message = "该笔盘点已保存，无需重复提交" };
        }
        await _repository.InsertAsync(record, autoSave: true);
        return new ResponseDto { success = true, message = record.TotalQuantity.HasValue
            ? $"盘点保存成功，实际数量{record.TotalQuantity}只" : "盘点保存成功，未填写的字段已留空" };
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAsync(string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var start) || start == DateTime.MaxValue.Date)
            throw new UserFriendlyException("请选择有效的导出日期，格式为yyyy-MM-dd");
        var end = start.AddDays(1);
        var records = await _repository.GetListAsync(x => x.RecordedAt >= start && x.RecordedAt < end);
        if (records.Count == 0) throw new UserFriendlyException("所选日期没有盘点数据");

        using var workbook = new XSSFWorkbook();
        var headers = new[] { "物料码", "产商代号", "等级", "整箱数量（只/箱）", "实际箱数（箱）", "实际零头（只）", "总数（只）", "录入时间" };
        // 每张表重新写入列头；超过单表容量时继续分表，保留全部明细。
        NPOI.SS.UserModel.ISheet AddSheet()
        {
            var next = workbook.CreateSheet(workbook.NumberOfSheets == 0 ? "盘点数据" : $"盘点数据{workbook.NumberOfSheets + 1}");
            var header = next.CreateRow(0);
            for (var i = 0; i < headers.Length; i++)
            {
                header.CreateCell(i).SetCellValue(headers[i]);
                next.SetColumnWidth(i, 22 * 256);
            }
            return next;
        }
        var sheet = AddSheet();
        var rowIndex = 1;
        foreach (var record in records.OrderBy(x => x.RecordedAt).ThenBy(x => x.Id))
        {
            // 超过Excel单表容量时分表，确保当天所有已保存明细均能导出。
            if (rowIndex == 1048576)
            {
                sheet = AddSheet();
                rowIndex = 1;
            }
            var row = sheet.CreateRow(rowIndex++);
            row.CreateCell(0).SetCellValue(record.MaterialCode);
            row.CreateCell(1).SetCellValue(record.ManufacturerCode);
            row.CreateCell(2).SetCellValue(record.Grade);
            // 空数量保留空单元格，实际填写的0仍以数字0导出。
            var perBox = row.CreateCell(3);
            if (record.QuantityPerBox.HasValue) perBox.SetCellValue(record.QuantityPerBox.Value);
            var boxes = row.CreateCell(4);
            if (record.ActualBoxes.HasValue) boxes.SetCellValue(record.ActualBoxes.Value);
            var remainder = row.CreateCell(5);
            if (record.RemainderQuantity.HasValue) remainder.SetCellValue(record.RemainderQuantity.Value);
            var total = row.CreateCell(6);
            if (record.TotalQuantity.HasValue) total.SetCellValue(record.TotalQuantity.Value);
            // 使用原始保存时间，以四位年、点分日期及24小时制显示到秒。
            row.CreateCell(7).SetCellValue(record.RecordedAt.ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
        using var stream = new MemoryStream();
        workbook.Write(stream, leaveOpen: true);
        return stream.ToArray();
    }
}
