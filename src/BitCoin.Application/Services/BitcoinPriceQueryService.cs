using BitCoin.Application.Abstractions;
using BitCoin.Application.Constants;
using BitCoin.Domain;

namespace BitCoin.Application.Services;

/// <summary>
/// Reads Bitcoin price snapshots from the cache and enforces API-facing read rules.
/// </summary>
public sealed class BitcoinPriceQueryService : IBitcoinPriceQueryService
{
    private static readonly IReadOnlyList<BitCoinPriceIndexHistoryModel> EmptyResult = [];

    private readonly ICacheProvider _cacheProvider;

    public BitcoinPriceQueryService(ICacheProvider cacheProvider)
    {
        ArgumentNullException.ThrowIfNull(cacheProvider);

        _cacheProvider = cacheProvider;
    }

    public async Task<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheProvider
            .TryGetValueAsync<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(CacheKeys.ApiLatest, cancellationToken)
            .ConfigureAwait(false);

        return cached is { Count: > 0 } ? cached : EmptyResult;
    }
}
