using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using BitCoin.Domain;

using Microsoft.Extensions.Caching.Hybrid;

namespace BitCoin.Infrastructure.Serialization;

/// <summary>
/// Wires <see cref="HybridCache"/> to the same AOT-safe, source-generated JSON contract
/// (<see cref="BitcoinApiJsonSerializerContext"/>) already used for the Redis payload and the
/// controller response, instead of HybridCache's reflection-based default serializer.
/// </summary>
public sealed class BitcoinHybridCacheSerializer : IHybridCacheSerializer<IReadOnlyList<BitCoinPriceIndexHistoryModel>>
{
    private static readonly JsonTypeInfo<IReadOnlyList<BitCoinPriceIndexHistoryModel>> TypeInfo =
        (JsonTypeInfo<IReadOnlyList<BitCoinPriceIndexHistoryModel>>)BitcoinApiJsonSerializerContext.Default
            .GetTypeInfo(typeof(IReadOnlyList<BitCoinPriceIndexHistoryModel>))!;

    public IReadOnlyList<BitCoinPriceIndexHistoryModel> Deserialize(ReadOnlySequence<byte> source)
    {
        var reader = new Utf8JsonReader(source);
        return JsonSerializer.Deserialize(ref reader, TypeInfo)!;
    }

    public void Serialize(IReadOnlyList<BitCoinPriceIndexHistoryModel> value, IBufferWriter<byte> target)
    {
        using var writer = new Utf8JsonWriter(target);
        JsonSerializer.Serialize(writer, value, TypeInfo);
    }
}
