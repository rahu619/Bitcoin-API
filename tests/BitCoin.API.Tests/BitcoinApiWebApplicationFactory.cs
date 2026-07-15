using BitCoin.API.Tests.Fakes;
using BitCoin.Application.Abstractions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BitCoin.API.Tests;

/// <summary>
/// Hosts the real Api app in-process with the outbound dependencies (Redis, CoinGecko) replaced
/// by in-memory fakes, so controller/auth tests don't need Docker or network access.
/// </summary>
internal sealed class BitcoinApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ValidJwtKey = "unit-test-signing-key-at-least-32-characters-long";
    public const string ValidJwtIssuer = "BitCoin.API.Tests";
    public const string ValidJwtAudience = "BitCoin.API.Tests.Clients";

    public FakeCacheProvider CacheProvider { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:cache", "localhost:6379");
        builder.UseSetting("ExternalAPISettings:Interval", "3600");
        builder.UseSetting("ExternalAPISettings:Count", "5");
        builder.UseSetting("ExternalAPISettings:Url:Base", "https://api.coingecko.com/api/v3");
        builder.UseSetting("ExternalAPISettings:Url:HistoricalPath", "/coins/bitcoin/market_chart?vs_currency=usd&days=30&interval=daily");
        builder.UseSetting("Jwt:Key", ValidJwtKey);
        builder.UseSetting("Jwt:Issuer", ValidJwtIssuer);
        builder.UseSetting("Jwt:Audience", ValidJwtAudience);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICacheProvider>();
            services.AddSingleton<ICacheProvider>(CacheProvider);

            services.RemoveAll<IBitcoinPriceIndexClient>();
            services.AddSingleton<IBitcoinPriceIndexClient, FakeBitcoinPriceIndexClient>();
        });
    }
}
