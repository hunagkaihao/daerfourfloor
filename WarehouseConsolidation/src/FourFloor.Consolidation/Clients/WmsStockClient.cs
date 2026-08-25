using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Models.Wms;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Clients;

public sealed class WmsStockClient(HttpClient httpClient, IOptions<WmsApiOptions> options)
    : WmsClientBase(httpClient, options)
{
    public Task<List<WmsStockDto>> GetStocksAsync(CancellationToken cancellationToken) =>
        PostJsonAsync<List<WmsStockDto>>(
            Options.StocksPath,
            new
            {
                SkipCount = 0,
                MaxResultCount = Math.Max(Options.PageSize, 100_000)
            },
            cancellationToken);

    public Task<WmsOperationResult> CreateMoveTaskAsync(
        string boxCode,
        string startCellCode,
        string endCellCode,
        CancellationToken cancellationToken) =>
        PostQueryAsync<WmsOperationResult>(
            Options.CreateMoveTaskPath,
            new Dictionary<string, string?>
            {
                ["boxCode"] = boxCode,
                ["startCellCode"] = startCellCode,
                ["endCellCode"] = endCellCode
            },
            cancellationToken);
}
