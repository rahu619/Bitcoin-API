using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Application.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitCoin.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ExternalAPISettings>()
            .Bind(configuration.GetSection("ExternalAPISettings"))
            .Validate(
                settings => settings.Interval > 0,
                "The polling interval must be greater than zero.")
            .Validate(
                settings => settings.Count > 0,
                "The number of results to retrieve must be greater than zero.")
            .Validate(
                settings => settings.Url is not null,
                "The URL configuration must be provided.")
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Url?.HistoricalPath),
                "The external API historical path must be configured.")
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Url?.Base),
                "The external API base URL must be configured.")
            .ValidateOnStart();

        services.AddSingleton<IBitcoinPriceQueryService, BitcoinPriceQueryService>();
        services.AddSingleton<IBitcoinPriceSyncService, BitcoinPriceSyncService>();

        return services;
    }
}
