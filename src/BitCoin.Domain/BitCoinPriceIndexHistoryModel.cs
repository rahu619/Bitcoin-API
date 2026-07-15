namespace BitCoin.Domain;

/// <summary>
/// A single Bitcoin price observation.
/// </summary>
public sealed record BitCoinPriceIndexHistoryModel
{
    /// <summary>
    /// The observation date, formatted as yyyy-MM-dd.
    /// </summary>
    public string? Date { get; init; }

    /// <summary>
    /// The US Dollar value.
    /// </summary>
    public decimal USD { get; init; }
}
