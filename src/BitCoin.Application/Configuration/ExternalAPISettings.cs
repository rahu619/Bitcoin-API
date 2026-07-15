namespace BitCoin.Application.Configuration;

/// <summary>
/// The external API configuration entity.
/// </summary>
public sealed class ExternalAPISettings
{
    /// <summary>
    /// The interval for invoking the BitCoin API, in seconds.
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// The number of results to take from the resultset.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The API URLs.
    /// </summary>
    public ExternalApiUrl? Url { get; set; }
}

public sealed class ExternalApiUrl
{
    /// <summary>
    /// The base URL.
    /// </summary>
    public string? Base { get; set; }

    /// <summary>
    /// The relative path (including query string) for the historical Bitcoin values endpoint,
    /// resolved against <see cref="Base"/> by the HTTP client at request time.
    /// </summary>
    public string? HistoricalPath { get; set; }
}
