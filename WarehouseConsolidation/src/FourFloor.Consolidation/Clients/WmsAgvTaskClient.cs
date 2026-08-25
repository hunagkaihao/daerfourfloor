using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Wms;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Clients;

public sealed class WmsAgvTaskClient(HttpClient httpClient, IOptions<WmsApiOptions> options)
    : WmsClientBase(httpClient, options)
{
    public Task<AgvPagedResult> GetTasksAsync(
        string? boxCode,
        int? status,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken) =>
        PostJsonAsync<AgvPagedResult>(
            Options.AgvTasksPath,
            new
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                BoxCode = boxCode,
                AgvTaskStatus = status
            },
            cancellationToken);

    public async Task<List<WmsAgvTaskDto>> GetActiveTasksAsync(CancellationToken cancellationToken)
    {
        var pageSize = Math.Max(Options.PageSize, 1000);
        var pageIndex = 1;
        var items = new List<WmsAgvTaskDto>();
        while (true)
        {
            var page = await GetTasksAsync(null, null, pageIndex, pageSize, cancellationToken);
            items.AddRange(page.Items);
            if (page.Items.Count == 0 || items.Count >= page.TotalCount)
            {
                break;
            }

            pageIndex++;
        }

        return items
            .Where(task => task.AgvTaskStatus is >= 0 and < 9)
            .ToList();
    }

    public async Task<WmsAgvTaskDto?> FindLatestTaskAsync(
        string boxCode,
        string fromCell,
        string toCell,
        DateTime submittedAfterUtc,
        CancellationToken cancellationToken)
    {
        var page = await GetTasksAsync(boxCode, null, 1, 100, cancellationToken);
        return page.Items
            .Where(task => string.Equals(task.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase))
            .Where(task => string.Equals(task.StartPositionCode, fromCell, StringComparison.OrdinalIgnoreCase))
            .Where(task => string.Equals(task.EndPositionCode, toCell, StringComparison.OrdinalIgnoreCase))
            .Where(task => !task.CreationTime.HasValue || task.CreationTime.Value.ToUniversalTime() >= submittedAfterUtc.AddMinutes(-1))
            .OrderByDescending(task => task.CreationTime)
            .ThenByDescending(task => task.Id)
            .FirstOrDefault();
    }
}
