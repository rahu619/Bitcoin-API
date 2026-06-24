using System.Collections.Generic;
using System.Text.Json.Serialization;

using BitCoin.API.Models;

namespace BitCoin.API.Serialization;

[JsonSerializable(typeof(BitCoinPriceIndexModel))]
[JsonSerializable(typeof(IReadOnlyList<BitCoinPriceIndexHistoryModel>))]
[JsonSerializable(typeof(List<BitCoinPriceIndexHistoryModel>))]
internal sealed partial class BitcoinApiJsonSerializerContext : JsonSerializerContext { }
