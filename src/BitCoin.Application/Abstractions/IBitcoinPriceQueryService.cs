using BitCoin.Domain;

namespace BitCoin.Application.Abstractions;

/// <summary>
/// Application-facing query abstraction for retrieving the latest cached Bitcoin prices.
/// </summary>
public interface IBitcoinPriceQueryService
{
    /// <summary>
    /// Retrieves the latest cached Bitcoin price snapshot. Returns an empty list when nothing is cached yet.
    /// </summary>
    public Task<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetLatestAsync(CancellationToken cancellationToken = default);
}
