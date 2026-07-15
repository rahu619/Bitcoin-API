using BitCoin.Application.Configuration;
using BitCoin.Domain;
using BitCoin.Infrastructure.Caching;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Testcontainers.Redis;

namespace BitCoin.Infrastructure.Tests;

[TestClass]
public sealed class RedisCacheProviderTests
{
    private static RedisContainer? _container;
    private static IDistributedCache? _distributedCache;

    [ClassInitialize]
    public static async Task InitializeAsync(TestContext _)
    {
        _container = new RedisBuilder("redis:7.4").Build();
        await _container.StartAsync();

        var redisCache = new RedisCache(Options.Create(new RedisCacheOptions
        {
            Configuration = _container.GetConnectionString()
        }));
        _distributedCache = redisCache;
    }

    [ClassCleanup]
    public static async Task CleanupAsync()
    {
        if (_distributedCache is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task SetThenGet_RoundTripsTheSameValue()
    {
        var provider = CreateProvider(intervalSeconds: 60);
        var key = $"test:{Guid.NewGuid()}";
        IReadOnlyList<BitCoinPriceIndexHistoryModel> value = [new() { Date = "2026-07-14", USD = 65000.12m }];

        await provider.SetValueAsync(key, value);
        var result = await provider.TryGetValueAsync<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(key);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Count);
        Assert.AreEqual("2026-07-14", result[0].Date);
        Assert.AreEqual(65000.12m, result[0].USD);
    }

    [TestMethod]
    public async Task TryGetValueAsync_ReturnsDefault_WhenKeyDoesNotExist()
    {
        var provider = CreateProvider(intervalSeconds: 60);

        var result = await provider.TryGetValueAsync<string>($"missing:{Guid.NewGuid()}");

        Assert.IsNull(result);
    }

    private static RedisCacheProvider CreateProvider(int intervalSeconds)
    {
        var settings = Options.Create(new ExternalAPISettings { Interval = intervalSeconds, Count = 5 });
        return new RedisCacheProvider(NullLogger<RedisCacheProvider>.Instance, _distributedCache!, settings);
    }
}
