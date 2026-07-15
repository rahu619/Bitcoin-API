var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume();

// Sits between the API and the Aspire dashboard's OTLP endpoint: tail-samples traces
// (keep every error, every slow request, plus a 50% baseline of the rest) before they're
// stored, so the dashboard doesn't fill up with routine 200-OK noise. See otel-collector-config.yaml.
#pragma warning disable ASPIREATS001 // OpenTelemetry Collector integration is experimental
var otelCollector = builder.AddOpenTelemetryCollector("otel-collector", settings =>
    {
        settings.EnableHttpEndpoint = false;
        settings.ForceNonSecureReceiver = true;
    })
    .WithConfig("./otel-collector-config.yaml")
    .WithAppForwarding();
#pragma warning restore ASPIREATS001

builder.AddProject<Projects.BitCoin_API>("bitcoin-api")
    .WithReference(cache)
    .WaitFor(cache)
    .WaitFor(otelCollector)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
