using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.Extensions.Options;

namespace BitCoin.API.Services;

/// <summary>
/// Typed HTTP client responsible for retrieving Bitcoin price index payloads.
/// </summary>
public sealed class BitcoinPriceIndexClient : IBitcoinPriceIndexClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _historicalEndpoint;

    public BitcoinPriceIndexClient(HttpClient httpClient, IOptions<ExternalAPISettings> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        ArgumentNullException.ThrowIfNull(value);

        var historicalEndpoint = value.Url?.Historical;
        if (string.IsNullOrWhiteSpace(historicalEndpoint))
        {
            throw new InvalidOperationException("The historical endpoint is not configured.");
        }

        _httpClient = httpClient;
        _historicalEndpoint = historicalEndpoint;
    }

    /// <inheritdoc />
    public async Task<BitCoinPriceIndexModel?> GetHistoricalAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync(_historicalEndpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<BitCoinPriceIndexModel>(contentStream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
