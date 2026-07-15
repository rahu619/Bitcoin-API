using System.Text.Json.Serialization;

namespace BitCoin.Infrastructure.Http;

/// <summary>
/// Covers the raw CoinGecko wire DTO only. Internal — kept out of the public
/// <see cref="Serialization.BitcoinApiJsonSerializerContext"/> since <see cref="BitcoinPriceIndexResponse"/>
/// never crosses the <c>IBitcoinPriceIndexClient</c> port boundary.
/// </summary>
[JsonSerializable(typeof(BitcoinPriceIndexResponse))]
internal sealed partial class BitcoinPriceIndexResponseJsonSerializerContext : JsonSerializerContext;
