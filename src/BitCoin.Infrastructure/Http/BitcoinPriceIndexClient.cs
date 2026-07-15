using System.Globalization;
using System.Text.Json;

using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.Infrastructure.Http;

/// <summary>
/// Typed HTTP client responsible for retrieving Bitcoin price index payloads and mapping them
/// to the domain shape. This is the anti-corruption layer between CoinGecko's wire format and
/// the rest of the application — <see cref="BitcoinPriceIndexResponse"/> never leaves this class.
/// </summary>
public sealed partial class BitcoinPriceIndexClient : IBitcoinPriceIndexClient
{
    private static readonly IReadOnlyList<BitCoinPriceIndexHistoryModel> EmptyResult = [];

    private readonly HttpClient _httpClient;
    private readonly Uri _historicalRequestUri;
    private readonly ILogger<BitcoinPriceIndexClient> _logger;

    public BitcoinPriceIndexClient(
        HttpClient httpClient,
        ILogger<BitcoinPriceIndexClient> logger,
        IOptions<ExternalAPISettings> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        var settings = options.Value;
        var baseUrl = settings.Url?.Base;
        var historicalPath = settings.Url?.HistoricalPath;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("The external API base URL must be configured.");
        }

        if (string.IsNullOrWhiteSpace(historicalPath))
        {
            throw new InvalidOperationException("The external API historical path must be configured.");
        }

        _httpClient = httpClient;
        _logger = logger;

        // Plain concatenation, not `new Uri(baseUri, relativePath)`: HistoricalPath starts with '/', and
        // RFC 3986 relative-reference resolution treats a leading '/' as rooted at the host, which would
        // silently drop Base's own path segment (e.g. "/api/v3").
        _historicalRequestUri = new Uri(baseUrl.TrimEnd('/') + historicalPath, UriKind.Absolute);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetHistoricalAsync(CancellationToken cancellationToken = default)
    {
        Log.RequestingHistoricalData(_logger, _historicalRequestUri);

        using var response = await _httpClient
            .GetAsync(_historicalRequestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var payload = await JsonSerializer
            .DeserializeAsync(contentStream, BitcoinPriceIndexResponseJsonSerializerContext.Default.BitcoinPriceIndexResponse, cancellationToken)
            .ConfigureAwait(false);

        var history = MapToHistory(payload);
        Log.ReceivedHistoricalData(_logger, history.Count);
        return history;
    }

    /// <summary>
    /// Maps the raw [timestamp, price] pairs to domain models, ordered newest first.
    /// </summary>
    private static IReadOnlyList<BitCoinPriceIndexHistoryModel> MapToHistory(BitcoinPriceIndexResponse? payload)
    {
        if (payload?.Prices is not { Count: > 0 } pricePoints)
        {
            return EmptyResult;
        }

        return [.. pricePoints
            .Where(point => point.Length == 2)
            .OrderByDescending(point => point[0])
            .Select(point => new BitCoinPriceIndexHistoryModel
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds((long)point[0]).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                USD = (decimal)point[1]
            })];
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 3001, Level = LogLevel.Debug, Message = "Requesting Bitcoin historical data from '{RequestUri}'.")]
        public static partial void RequestingHistoricalData(ILogger logger, Uri requestUri);

        [LoggerMessage(EventId = 3002, Level = LogLevel.Debug, Message = "Received {Count} historical price points.")]
        public static partial void ReceivedHistoricalData(ILogger logger, int count);
    }
}
