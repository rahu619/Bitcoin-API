using System.Diagnostics;

using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Application.Constants;
using BitCoin.Application.Diagnostics;
using BitCoin.Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.Application.Services;

/// <summary>
/// Orchestrates a single fetch-map-cache cycle: fetches price history from the external client,
/// applies the configured result-count policy, records telemetry, and writes the result to the cache.
/// Scheduling (when/how often to call this) is a hosting concern that lives outside this service.
/// </summary>
public sealed partial class BitcoinPriceSyncService : IBitcoinPriceSyncService
{
    private readonly IBitcoinPriceIndexClient _priceIndexClient;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<BitcoinPriceSyncService> _logger;
    private readonly ExternalAPISettings _apiSettings;

    public BitcoinPriceSyncService(
        IBitcoinPriceIndexClient priceIndexClient,
        ICacheProvider cacheProvider,
        ILogger<BitcoinPriceSyncService> logger,
        IOptions<ExternalAPISettings> apiSettings)
    {
        ArgumentNullException.ThrowIfNull(priceIndexClient);
        ArgumentNullException.ThrowIfNull(cacheProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(apiSettings);

        _priceIndexClient = priceIndexClient;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _apiSettings = apiSettings.Value;
    }

    public async Task SyncOnceAsync(CancellationToken cancellationToken = default)
    {
        using var activity = BitcoinApiTelemetry.ActivitySource.StartActivity(
            "BitcoinPriceIndex.Fetch", ActivityKind.Internal);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var history = await _priceIndexClient.GetHistoricalAsync(cancellationToken).ConfigureAwait(false);
            var result = ApplyResultCountPolicy(history);

            if (result.Count == 0)
            {
                activity?.SetTag("bitcoin.result_count", 0);
                activity?.SetStatus(ActivityStatusCode.Error, "No price index data retrieved.");
                BitcoinApiTelemetry.RecordFetch(success: false, stopwatch.Elapsed.TotalMilliseconds);
                Log.NoPriceIndexDataRetrieved(_logger);
                return;
            }

            await _cacheProvider.SetValueAsync(CacheKeys.ApiLatest, result, cancellationToken).ConfigureAwait(false);

            activity?.SetTag("bitcoin.result_count", result.Count);
            activity?.SetTag("bitcoin.latest_price_usd", result[0].USD);
            activity?.SetStatus(ActivityStatusCode.Ok);
            BitcoinApiTelemetry.RecordFetch(success: true, stopwatch.Elapsed.TotalMilliseconds);
            Log.FetchedItemCount(_logger, result.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            BitcoinApiTelemetry.RecordFetch(success: false, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Applies the configured result-count policy to an already date-ordered (newest first) history list.
    /// </summary>
    private IReadOnlyList<BitCoinPriceIndexHistoryModel> ApplyResultCountPolicy(IReadOnlyList<BitCoinPriceIndexHistoryModel> history)
    {
        if (history.Count == 0)
        {
            Log.MappingSourceDataMissing(_logger);
            return history;
        }

        if (_apiSettings.Count <= 0)
        {
            Log.ConfiguredResultCountInvalid(_logger);
            return [];
        }

        var resultCount = Math.Min(_apiSettings.Count, history.Count);
        return resultCount == history.Count ? history : [.. history.Take(resultCount)];
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "No Bitcoin price index data was retrieved.")]
        public static partial void NoPriceIndexDataRetrieved(ILogger logger);

        [LoggerMessage(EventId = 1005, Level = LogLevel.Debug, Message = "Fetched {Count} items from Bitcoin API.")]
        public static partial void FetchedItemCount(ILogger logger, int count);

        [LoggerMessage(EventId = 1006, Level = LogLevel.Error, Message = "No data found while mapping Bitcoin API response.")]
        public static partial void MappingSourceDataMissing(ILogger logger);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Warning, Message = "Configured result count is not positive. No data will be cached.")]
        public static partial void ConfiguredResultCountInvalid(ILogger logger);
    }
}
