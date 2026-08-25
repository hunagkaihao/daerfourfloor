using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FourFloor.Consolidation.Configuration;
using Microsoft.Extensions.Options;

namespace FourFloor.Consolidation.Clients;

public abstract class WmsClientBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    protected WmsClientBase(HttpClient httpClient, IOptions<WmsApiOptions> options)
    {
        HttpClient = httpClient;
        Options = options.Value;

        if (!string.IsNullOrWhiteSpace(Options.BearerToken))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Options.BearerToken);
        }
    }

    protected HttpClient HttpClient { get; }
    protected WmsApiOptions Options { get; }

    protected async Task<T> PostJsonAsync<T>(string path, object payload, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.PostAsJsonAsync(path, payload, JsonOptions, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new WmsApiException(path, (int)response.StatusCode, responseBody);
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions)
               ?? throw new WmsApiException(path, (int)response.StatusCode, "WMS 返回了空响应。");
    }

    protected async Task<T> PostQueryAsync<T>(
        string path,
        IReadOnlyDictionary<string, string?> query,
        CancellationToken cancellationToken)
    {
        var queryText = string.Join("&", query
            .Where(item => item.Value is not null)
            .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));
        var requestUri = string.IsNullOrWhiteSpace(queryText) ? path : $"{path}?{queryText}";

        using var response = await HttpClient.PostAsync(requestUri, null, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new WmsApiException(requestUri, (int)response.StatusCode, responseBody);
        }

        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions)
               ?? throw new WmsApiException(requestUri, (int)response.StatusCode, "WMS 返回了空响应。");
    }
}

public sealed class WmsApiException(string path, int statusCode, string responseBody)
    : Exception($"调用 WMS 接口 {path} 失败，HTTP {statusCode}：{responseBody}")
{
    public string Path { get; } = path;
    public int StatusCode { get; } = statusCode;
    public string ResponseBody { get; } = responseBody;
}
