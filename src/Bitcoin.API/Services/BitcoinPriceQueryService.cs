using System;
using System.Collections.Generic;

using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;

namespace BitCoin.API.Services;

/// <summary>
/// Reads Bitcoin price snapshots from the cache and enforces API-facing read rules.
/// </summary>
public sealed class BitcoinPriceQueryService : IBitcoinPriceQueryService
{
    private static readonly IReadOnlyList<BitCoinPriceIndexHistoryModel> EmptyResult = Array.Empty<BitCoinPriceIndexHistoryModel>();

    private readonly ICacheProvider _cacheProvider;

    public BitcoinPriceQueryService(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public bool TryGetLatest(out IReadOnlyList<BitCoinPriceIndexHistoryModel> latestPrices)
    {
        if (_cacheProvider.TryGetValue<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(CacheKeys.ApiLatest, out var cached)
            && cached is { Count: > 0 })
        {
            latestPrices = cached;
            return true;
        }

        latestPrices = EmptyResult;
        return false;
    }
}