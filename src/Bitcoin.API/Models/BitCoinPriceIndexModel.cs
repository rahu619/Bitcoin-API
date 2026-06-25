using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitCoin.API.Models;

/// <summary>
/// The DTO for Bitcoin price index model.
/// </summary>
public sealed record class BitCoinPriceIndexModel
{
    /// <summary>
    /// Collection of historical data
    /// </summary>
    [JsonPropertyName("bpi")]
    public IReadOnlyDictionary<string, decimal>? BitCoinPriceIndexHistory { get; init; }

    /// <summary>
    /// Disclaimer detail
    /// </summary>
    [JsonPropertyName("disclaimer")]
    public string? Disclaimer { get; init; }

    /// <summary>
    /// Published time details
    /// </summary>
    [JsonPropertyName("time")]
    public BitCoinPriceIndexTimeModel? BitCoinPriceIndexTime { get; init; }
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

/// <summary>
/// DTO for updated time information
/// </summary>
public sealed record class BitCoinPriceIndexTimeModel
{
    /// <summary>
    /// The published datetime
    /// </summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; init; }

    /// <summary>
    /// The publiched datetime in ISO string format.
    /// </summary>
    [JsonPropertyName("updatedISO")]
    public string? UpdatedISO { get; init; }
}
