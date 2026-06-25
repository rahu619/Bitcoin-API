using System.Collections.Generic;

using BitCoin.API.Models;

namespace BitCoin.API.Interfaces;

/// <summary>
/// Application-facing query abstraction for retrieving the latest cached Bitcoin prices.
/// </summary>
public interface IBitcoinPriceQueryService
{
    /// <summary>
    /// Tries to retrieve the latest non-empty Bitcoin price snapshot.
    /// </summary>
    bool TryGetLatest(out IReadOnlyList<BitCoinPriceIndexHistoryModel> latestPrices);
}