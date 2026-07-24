using System.Text;

using BitCoin.API.BackgroundServices;
using BitCoin.API.Configuration;
using BitCoin.Application.DependencyInjection;
using BitCoin.Domain;
using BitCoin.Infrastructure.DependencyInjection;
using BitCoin.Infrastructure.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register the app-specific ActivitySource/Meter so the background fetch spans
// and business metrics (bitcoin.price_fetch.*, bitcoin.price.latest) surface in the Aspire dashboard.
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(BitCoin.Application.Diagnostics.BitcoinApiTelemetry.ActivitySourceName))
    .WithMetrics(metrics => metrics.AddMeter(BitCoin.Application.Diagnostics.BitcoinApiTelemetry.MeterName));

// Redis-backed IDistributedCache, provisioned by the AppHost "cache" resource.
// Wires up health checks and tracing/logging automatically (Aspire.StackExchange.Redis.DistributedCaching).
builder.AddRedisDistributedCache("cache");

// HybridCache layers an in-process L1 in front of the Redis L2 registered above (auto-detected),
// so concurrent requests for the same key are served from memory and coalesced instead of each
// making its own Redis round-trip.
builder.Services.AddHybridCache()
    .AddSerializer<IReadOnlyList<BitCoinPriceIndexHistoryModel>, BitcoinHybridCacheSerializer>();

builder.Services
    .AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection("Cors"))
    .ValidateOnStart();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// MVC controllers currently do not support Native AOT trimming. This is a known
// framework limitation. See: https://aka.ms/aspnet/trimming
#pragma warning disable IL2026
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, BitcoinApiJsonSerializerContext.Default));
#pragma warning restore IL2026
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetRequiredSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT authentication settings are missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Key) && settings.Key.Length >= 32,
        "The JWT key must be at least 32 characters.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Issuer),
        "The JWT issuer must be configured.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Audience),
        "The JWT audience must be configured.")
    .ValidateOnStart();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddHostedService<BitcoinPriceSyncBackgroundService>();

var app = builder.Build();
var runningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    bool.TrueString,
    StringComparison.OrdinalIgnoreCase);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BitCoin.API v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

if (!app.Environment.IsDevelopment() && !runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseHttpLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/healthz", static () => TypedResults.Ok()).AllowAnonymous();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();

/// <summary>
/// Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can host this app in-process for integration tests.
/// </summary>
public partial class Program;
