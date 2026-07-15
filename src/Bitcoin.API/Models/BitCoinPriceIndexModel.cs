using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitCoin.API.Models;

/// <summary>
/// The DTO for the CoinGecko market chart response (https://api.coingecko.com/api/v3/coins/bitcoin/market_chart).
/// No API key is required for this endpoint on the free tier.
/// </summary>
public sealed record class BitCoinPriceIndexModel
{
    /// <summary>
    /// Historical price points as [unixTimestampMs, priceUsd] pairs, newest last.
    /// </summary>
    [JsonPropertyName("prices")]
    public IReadOnlyList<double[]>? Prices { get; init; }
}

/// <summary>
/// DTO for bitcoin history
/// </summary>
public sealed record class BitCoinPriceIndexHistoryModel
{
    /// <summary>
    /// The updated date
    /// </summary>
    public string? Date { get; init; }

    /// <summary>
    /// The US Dollar value
    /// </summary>
    public decimal USD { get; init; }
}
