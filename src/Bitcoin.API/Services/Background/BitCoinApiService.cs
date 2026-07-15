using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Diagnostics;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.API.Services;

/// <summary>
/// Hosted service that periodically retrieves Bitcoin price index data and caches the result.
/// </summary>
public sealed partial class BitCoinApiService : BackgroundService
{
    private static readonly IReadOnlyList<BitCoinPriceIndexHistoryModel> EmptyResult = Array.Empty<BitCoinPriceIndexHistoryModel>();

    private readonly ICacheProvider _cacheProvider;
    private readonly IBitcoinPriceIndexClient _priceIndexClient;
    private readonly ILogger<BitCoinApiService> _logger;
    private readonly ExternalAPISettings _apiSettings;

    private IReadOnlyList<BitCoinPriceIndexHistoryModel> _latestResult = EmptyResult;

    /// <summary>
    /// Gets the most recently retrieved Bitcoin price index snapshot.
    /// </summary>
    internal IReadOnlyList<BitCoinPriceIndexHistoryModel> LatestResult => Volatile.Read(ref _latestResult);

    public BitCoinApiService(
        IBitcoinPriceIndexClient priceIndexClient,
        ILogger<BitCoinApiService> logger,
        ICacheProvider cacheProvider,
        IOptions<ExternalAPISettings> apiSettings)
    {
        _priceIndexClient = priceIndexClient ?? throw new ArgumentNullException(nameof(priceIndexClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _apiSettings = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.BackgroundServiceStarting(_logger);

        try
        {
            using var timer = new PeriodicTimer(GetPollingInterval());

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteIterationAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.ErrorRetrievingPriceIndex(_logger, ex);
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    break;
                }
            }
        }
        finally
        {
            Log.BackgroundServiceStopped(_logger);
        }
    }

    /// <summary>
    /// Fetches the Bitcoin historical API details and stores the latest values in cache.
    /// </summary>
    internal async Task ExecuteIterationAsync(CancellationToken cancellationToken = default)
    {
        using var activity = BitcoinApiTelemetry.ActivitySource.StartActivity(
            "BitcoinPriceIndex.Fetch", ActivityKind.Internal);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var content = await _priceIndexClient.GetHistoricalAsync(cancellationToken).ConfigureAwait(false);
            var filtered = FilterContent(content);

            if (filtered.Count == 0)
            {
                activity?.SetTag("bitcoin.result_count", 0);
                activity?.SetStatus(ActivityStatusCode.Error, "No price index data retrieved.");
                BitcoinApiTelemetry.RecordFetch(success: false, stopwatch.Elapsed.TotalMilliseconds);
                Log.NoPriceIndexDataRetrieved(_logger);
                return;
            }

            Volatile.Write(ref _latestResult, filtered);
            _cacheProvider.SetValue(CacheKeys.ApiLatest, filtered);

            activity?.SetTag("bitcoin.result_count", filtered.Count);
            activity?.SetTag("bitcoin.latest_price_usd", filtered[0].USD);
            activity?.SetStatus(ActivityStatusCode.Ok);
            BitcoinApiTelemetry.RecordFetch(success: true, stopwatch.Elapsed.TotalMilliseconds);
            Log.FetchedItemCount(_logger, filtered.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            BitcoinApiTelemetry.RecordFetch(success: false, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Maps the obtained json content to <see cref="BitCoinPriceIndexHistoryModel"/> instances.
    /// </summary>
    /// <param name="data">The raw API response.</param>
    private IReadOnlyList<BitCoinPriceIndexHistoryModel> FilterContent(BitCoinPriceIndexModel? data)
    {
        if (data?.Prices is not { Count: > 0 } pricePoints)
        {
            Log.MappingSourceDataMissing(_logger);
            return EmptyResult;
        }

        if (_apiSettings.Count <= 0)
        {
            Log.ConfiguredResultCountInvalid(_logger);
            return EmptyResult;
        }

        Log.BitcoinApiTaskRunning(_logger);

        var resultCount = Math.Min(_apiSettings.Count, pricePoints.Count);
        var results = new List<BitCoinPriceIndexHistoryModel>(resultCount);

        foreach (var point in pricePoints
                     .Where(point => point.Length == 2)
                     .OrderByDescending(point => point[0])
                     .Take(resultCount))
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds((long)point[0]).UtcDateTime.ToString("yyyy-MM-dd");
            results.Add(new BitCoinPriceIndexHistoryModel
            {
                Date = date,
                USD = (decimal)point[1]
            });
        }

        return results;
    }

    private TimeSpan GetPollingInterval()
    {
        var seconds = Math.Max(1, _apiSettings.Interval);
        return TimeSpan.FromSeconds(seconds);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Starting Bitcoin API background service.")]
        public static partial void BackgroundServiceStarting(ILogger logger);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "An error occurred while retrieving Bitcoin price index data.")]
        public static partial void ErrorRetrievingPriceIndex(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Bitcoin API task stopped.")]
        public static partial void BackgroundServiceStopped(ILogger logger);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "No Bitcoin price index data was retrieved.")]
        public static partial void NoPriceIndexDataRetrieved(ILogger logger);

        [LoggerMessage(EventId = 1005, Level = LogLevel.Debug, Message = "Fetched {Count} items from Bitcoin API.")]
        public static partial void FetchedItemCount(ILogger logger, int count);

        [LoggerMessage(EventId = 1006, Level = LogLevel.Error, Message = "No data found while mapping Bitcoin API response.")]
        public static partial void MappingSourceDataMissing(ILogger logger);

        [LoggerMessage(EventId = 1007, Level = LogLevel.Warning, Message = "Configured result count is not positive. No data will be cached.")]
        public static partial void ConfiguredResultCountInvalid(ILogger logger);

        [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Bitcoin API task running.")]
        public static partial void BitcoinApiTaskRunning(ILogger logger);
    }
}
