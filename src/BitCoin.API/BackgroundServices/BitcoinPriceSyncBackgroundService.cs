using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.API.BackgroundServices;

/// <summary>
/// Owns the polling schedule only; delegates the actual fetch-map-cache work to
/// <see cref="IBitcoinPriceSyncService"/>. Kept thin and hosting-specific so the sync logic
/// itself is testable without an <see cref="IHostedService"/> in the loop.
/// </summary>
public sealed partial class BitcoinPriceSyncBackgroundService : BackgroundService
{
    private readonly IBitcoinPriceSyncService _syncService;
    private readonly ILogger<BitcoinPriceSyncBackgroundService> _logger;
    private readonly ExternalAPISettings _apiSettings;

    public BitcoinPriceSyncBackgroundService(
        IBitcoinPriceSyncService syncService,
        ILogger<BitcoinPriceSyncBackgroundService> logger,
        IOptions<ExternalAPISettings> apiSettings)
    {
        ArgumentNullException.ThrowIfNull(syncService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(apiSettings);

        _syncService = syncService;
        _logger = logger;
        _apiSettings = apiSettings.Value;
    }

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
                    await _syncService.SyncOnceAsync(stoppingToken).ConfigureAwait(false);
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

    private TimeSpan GetPollingInterval() => TimeSpan.FromSeconds(Math.Max(1, _apiSettings.Interval));

    private static partial class Log
    {
        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Starting Bitcoin API background service.")]
        public static partial void BackgroundServiceStarting(ILogger logger);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "An error occurred while retrieving Bitcoin price index data.")]
        public static partial void ErrorRetrievingPriceIndex(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Bitcoin API background service stopped.")]
        public static partial void BackgroundServiceStopped(ILogger logger);
    }
}
