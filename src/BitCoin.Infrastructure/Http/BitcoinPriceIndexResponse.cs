using System.Text.Json.Serialization;

namespace BitCoin.Infrastructure.Http;

/// <summary>
/// The raw DTO for the CoinGecko market chart response (https://api.coingecko.com/api/v3/coins/bitcoin/market_chart).
/// No API key is required for this endpoint on the free tier. Internal — never crosses the
/// <c>IBitcoinPriceIndexClient</c> port boundary; <see cref="BitcoinPriceIndexClient"/> maps it to
/// the domain <c>BitCoinPriceIndexHistoryModel</c> shape before returning.
/// </summary>
internal sealed record BitcoinPriceIndexResponse
{
    /// <summary>
    /// Historical price points as [unixTimestampMs, priceUsd] pairs, newest last.
    /// </summary>
    [JsonPropertyName("prices")]
    public IReadOnlyList<double[]>? Prices { get; init; }
}
