using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BitCoin.Application.Diagnostics;

/// <summary>
/// Tracing and metrics for the Bitcoin price sync use case — the one piece of this app
/// with no off-the-shelf OpenTelemetry instrumentation package (unlike HTTP/Redis, see ServiceDefaults).
/// Registered with OpenTelemetry in the Api's Program.cs so spans/metrics surface in the Aspire dashboard.
/// </summary>
public static class BitcoinApiTelemetry
{
    public const string ActivitySourceName = "BitCoin.API";
    public const string MeterName = "BitCoin.API";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> FetchCounter =
        Meter.CreateCounter<long>("bitcoin.price_fetch.count", unit: "{fetch}", description: "Bitcoin price fetch attempts, by outcome.");

    private static readonly Histogram<double> FetchDurationHistogram =
        Meter.CreateHistogram<double>("bitcoin.price_fetch.duration", unit: "ms", description: "Duration of a Bitcoin price fetch iteration, by outcome.");

    public static void RecordFetch(bool success, double durationMs)
    {
        var outcome = new KeyValuePair<string, object?>("outcome", success ? "success" : "failure");
        FetchCounter.Add(1, outcome);
        FetchDurationHistogram.Record(durationMs, outcome);
    }
}
