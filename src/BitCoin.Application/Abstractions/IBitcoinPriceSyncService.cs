namespace BitCoin.Application.Abstractions;

/// <summary>
/// Orchestrates a single fetch-map-cache cycle for Bitcoin price history.
/// Hosting concerns (scheduling, retry-on-tick) live outside this port.
/// </summary>
public interface IBitcoinPriceSyncService
{
    /// <summary>
    /// Fetches the latest price history, applies the configured result-count policy,
    /// records telemetry, and writes the result to the cache.
    /// </summary>
    public Task SyncOnceAsync(CancellationToken cancellationToken = default);
}
