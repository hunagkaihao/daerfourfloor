using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Wms;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Clients;

public sealed class WmsBoxClient(HttpClient httpClient, IOptions<WmsApiOptions> options)
    : WmsClientBase(httpClient, options)
{
    public async Task<List<WmsBoxDto>> GetAllBoxesAsync(CancellationToken cancellationToken)
    {
        var items = new List<WmsBoxDto>();
        var skipCount = 0;
        var pageSize = Math.Max(Options.PageSize, 100);

        while (true)
        {
            var page = await PostJsonAsync<PagedResult<WmsBoxDto>>(
                Options.BoxesPath,
                new
                {
                    SkipCount = skipCount,
                    MaxResultCount = pageSize
                },
                cancellationToken);
            items.AddRange(page.Items);
            skipCount += page.Items.Count;
            if (page.Items.Count == 0 || skipCount >= page.TotalCount)
            {
                return items;
            }
        }
    }
}
