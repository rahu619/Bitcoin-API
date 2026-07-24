using System.Text.Json.Serialization;

using BitCoin.Domain;

namespace BitCoin.Infrastructure.Serialization;

/// <summary>
/// Covers the domain-shaped types shared across the cache write path (<see cref="Caching.CacheProvider"/>)
/// and the Api's controller JSON output. Public so the Api project can reference it for AOT-safe MVC serialization.
/// </summary>
[JsonSerializable(typeof(IReadOnlyList<BitCoinPriceIndexHistoryModel>))]
[JsonSerializable(typeof(List<BitCoinPriceIndexHistoryModel>))]
public sealed partial class BitcoinApiJsonSerializerContext : JsonSerializerContext;
