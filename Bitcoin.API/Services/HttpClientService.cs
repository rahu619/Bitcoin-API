using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Interfaces;

namespace BitCoin.API.Services;

/// <summary>
/// A wrapper service for Http client
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class HttpClientService<T> : IHttpClientService<T>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public HttpClientService(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>
    /// Returns content from the url
    /// </summary>
    /// <param name="url">The endpoint to call.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    public async Task<T> GetContentAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("The request url must be provided.", nameof(url));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await _client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var result = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var content = await JsonSerializer.DeserializeAsync<T>(result, SerializerOptions, cancellationToken).ConfigureAwait(false);

        if (content is null)
        {
            throw new InvalidOperationException("Unable to deserialize the response content.");
        }

        return content;
    }
}
