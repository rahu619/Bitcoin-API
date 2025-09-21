using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.API.Services;

/// <summary>
/// Hosted service that periodically retrieves Bitcoin price index data and caches the result.
/// </summary>
public sealed class BitCoinApiService : BackgroundService
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
        _logger.LogInformation("Starting Bitcoin API background service.");

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
                    _logger.LogError(ex, "An error occurred while retrieving Bitcoin price index data.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    break;
                }
            }
        }
        finally
        {
            _logger.LogDebug("Bitcoin API task stopped.");
        }
    }

    /// <summary>
    /// Fetches the Bitcoin historical API details and stores the latest values in cache.
    /// </summary>
    internal async Task ExecuteIterationAsync(CancellationToken cancellationToken = default)
    {
        var content = await _priceIndexClient.GetHistoricalAsync(cancellationToken).ConfigureAwait(false);
        var filtered = FilterContent(content);

        if (filtered.Count == 0)
        {
            _logger.LogWarning("No Bitcoin price index data was retrieved.");
            return;
        }

        Volatile.Write(ref _latestResult, filtered);
        _cacheProvider.Set(CacheKeys.ApiLatest, filtered);
        _logger.LogDebug("Fetched {Count} items from Bitcoin API.", filtered.Count);
    }

    /// <summary>
    /// Maps the obtained json content to <see cref="BitCoinPriceIndexHistoryModel"/> instances.
    /// </summary>
    /// <param name="data">The raw API response.</param>
    private IReadOnlyList<BitCoinPriceIndexHistoryModel> FilterContent(BitCoinPriceIndexModel? data)
    {
        if (data?.BitCoinPriceIndexHistory is not { Count: > 0 } datePriceCollection)
        {
            _logger.LogError("No data found while mapping Bitcoin API response.");
            return EmptyResult;
        }

        if (_apiSettings.Count <= 0)
        {
            _logger.LogWarning("Configured result count is not positive. No data will be cached.");
            return EmptyResult;
        }

        _logger.LogInformation("Bitcoin API task running.");

        var resultCount = Math.Min(_apiSettings.Count, datePriceCollection.Count);
        var results = new List<BitCoinPriceIndexHistoryModel>(resultCount);

        foreach (var (date, value) in datePriceCollection
                     .OrderByDescending(pair => pair.Key, StringComparer.Ordinal)
                     .Take(resultCount))
        {
            results.Add(new BitCoinPriceIndexHistoryModel
            {
                Date = date,
                USD = value
            });
        }

        return results;
    }

    private TimeSpan GetPollingInterval()
    {
        var seconds = Math.Max(1, _apiSettings.Interval);
        return TimeSpan.FromSeconds(seconds);
    }
}
