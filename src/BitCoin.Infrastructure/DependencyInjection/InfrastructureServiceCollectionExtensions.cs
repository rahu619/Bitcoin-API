using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Infrastructure.Caching;
using BitCoin.Infrastructure.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BitCoin.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        services
            .AddHttpClient<IBitcoinPriceIndexClient, BitcoinPriceIndexClient>("BitcoinPriceIndex")
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ExternalAPISettings>>().Value;

                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                // CoinGecko's edge (Cloudflare) returns 403 for requests with no User-Agent header,
                // which is what HttpClient sends by default.
                client.DefaultRequestHeaders.UserAgent.ParseAdd("BitCoin.API/1.0 (+https://github.com/rahu619/Bitcoin-API)");
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
