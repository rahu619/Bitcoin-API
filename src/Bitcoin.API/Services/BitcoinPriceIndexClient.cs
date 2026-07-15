using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using BitCoin.API.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.API.Services;

/// <summary>
/// Typed HTTP client responsible for retrieving Bitcoin price index payloads.
/// </summary>
public sealed partial class BitcoinPriceIndexClient : IBitcoinPriceIndexClient
{
    private readonly HttpClient _httpClient;
    private readonly string _historicalEndpoint;
    private readonly ILogger<BitcoinPriceIndexClient> _logger;

    public BitcoinPriceIndexClient(
        HttpClient httpClient,
        ILogger<BitcoinPriceIndexClient> logger,
        IOptions<ExternalAPISettings> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        ArgumentNullException.ThrowIfNull(value);

        var historicalEndpoint = value.Url?.Historical;
        if (string.IsNullOrWhiteSpace(historicalEndpoint))
        {
            throw new InvalidOperationException("The historical endpoint is not configured.");
        }

        _httpClient = httpClient;
        _logger = logger;
        _historicalEndpoint = historicalEndpoint;
    }

    /// <inheritdoc />
    public async Task<BitCoinPriceIndexModel?> GetHistoricalAsync(CancellationToken cancellationToken = default)
    {
        Log.RequestingHistoricalData(_logger, _historicalEndpoint);

        using var response = await _httpClient
            .GetAsync(_historicalEndpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var payload = await JsonSerializer
            .DeserializeAsync(contentStream, BitcoinApiJsonSerializerContext.Default.BitCoinPriceIndexModel, cancellationToken)
            .ConfigureAwait(false);

        Log.ReceivedHistoricalData(_logger, payload?.Prices?.Count ?? 0);
        return payload;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 3001, Level = LogLevel.Debug, Message = "Requesting Bitcoin historical data from '{Endpoint}'.")]
        public static partial void RequestingHistoricalData(ILogger logger, string endpoint);

        [LoggerMessage(EventId = 3002, Level = LogLevel.Debug, Message = "Received {Count} historical price points.")]
        public static partial void ReceivedHistoricalData(ILogger logger, int count);
    }
}
