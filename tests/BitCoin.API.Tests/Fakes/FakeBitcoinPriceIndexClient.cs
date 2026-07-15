using BitCoin.Application.Abstractions;
using BitCoin.Domain;

namespace BitCoin.API.Tests.Fakes;

/// <summary>
/// Returns an empty history and never touches the network — used to keep the real
/// <see cref="BitCoin.API.BackgroundServices.BitcoinPriceSyncBackgroundService"/> harmless during
/// controller integration tests, which seed the cache directly instead of waiting on a poll tick.
/// </summary>
internal sealed class FakeBitcoinPriceIndexClient : IBitcoinPriceIndexClient
{
    public Task<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetHistoricalAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BitCoinPriceIndexHistoryModel>>([]);
}
