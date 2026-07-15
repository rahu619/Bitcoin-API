using BitCoin.Application.Configuration;
using BitCoin.Application.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitCoin.Application.Tests;

[TestClass]
public sealed class ExternalApiSettingsValidationTests
{
    [TestMethod]
    public void OptionsValue_Throws_WhenIntervalIsZero()
        => AssertInvalid(("ExternalAPISettings:Interval", "0"), ("ExternalAPISettings:Count", "5"),
            ("ExternalAPISettings:Url:Base", "https://api.coingecko.com/api/v3"),
            ("ExternalAPISettings:Url:HistoricalPath", "/coins/bitcoin/market_chart"));

    [TestMethod]
    public void OptionsValue_Throws_WhenCountIsZero()
        => AssertInvalid(("ExternalAPISettings:Interval", "60"), ("ExternalAPISettings:Count", "0"),
            ("ExternalAPISettings:Url:Base", "https://api.coingecko.com/api/v3"),
            ("ExternalAPISettings:Url:HistoricalPath", "/coins/bitcoin/market_chart"));

    [TestMethod]
    public void OptionsValue_Throws_WhenBaseUrlIsMissing()
        => AssertInvalid(("ExternalAPISettings:Interval", "60"), ("ExternalAPISettings:Count", "5"),
            ("ExternalAPISettings:Url:HistoricalPath", "/coins/bitcoin/market_chart"));

    [TestMethod]
    public void OptionsValue_Succeeds_WithValidConfiguration()
    {
        var provider = BuildServiceProvider(
            ("ExternalAPISettings:Interval", "60"), ("ExternalAPISettings:Count", "5"),
            ("ExternalAPISettings:Url:Base", "https://api.coingecko.com/api/v3"),
            ("ExternalAPISettings:Url:HistoricalPath", "/coins/bitcoin/market_chart"));

        var settings = provider.GetRequiredService<IOptions<ExternalAPISettings>>().Value;

        Assert.AreEqual(60, settings.Interval);
        Assert.AreEqual(5, settings.Count);
    }

    private static void AssertInvalid(params (string Key, string Value)[] configValues)
    {
        var provider = BuildServiceProvider(configValues);

        Assert.ThrowsExactly<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<ExternalAPISettings>>().Value);
    }

    private static ServiceProvider BuildServiceProvider(params (string Key, string Value)[] configValues)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)))
            .Build();

        var services = new ServiceCollection();
        services.AddApplication(configuration);

        return services.BuildServiceProvider();
    }
}
