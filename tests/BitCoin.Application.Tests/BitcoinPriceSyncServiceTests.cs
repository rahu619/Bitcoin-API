using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Application.Constants;
using BitCoin.Application.Services;
using BitCoin.Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace BitCoin.Application.Tests;

[TestClass]
public sealed class BitcoinPriceSyncServiceTests
{
    // Matches the ordering a real IBitcoinPriceIndexClient implementation would already have applied (newest first).
    private static readonly IReadOnlyList<BitCoinPriceIndexHistoryModel> ThreeDayHistory =
    [
        new() { Date = "2021-10-02", USD = 47071.0575m },
        new() { Date = "2021-10-01", USD = 43823.5533m },
        new() { Date = "2021-09-30", USD = 41501.6017m }
    ];

    [TestMethod]
    public async Task SyncOnceAsync_CachesTopNResults_AccordingToConfiguredCount()
    {
        var client = new Mock<IBitcoinPriceIndexClient>();
        client.Setup(c => c.GetHistoricalAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ThreeDayHistory);

        var cacheProvider = new Mock<ICacheProvider>();
        IReadOnlyList<BitCoinPriceIndexHistoryModel>? cached = null;
        cacheProvider
            .Setup(c => c.SetValueAsync(CacheKeys.ApiLatest, It.IsAny<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(), It.IsAny<CancellationToken>()))
            .Callback<string, IReadOnlyList<BitCoinPriceIndexHistoryModel>, CancellationToken>((_, value, _) => cached = value)
            .Returns(Task.CompletedTask);

        var settings = Options.Create(new ExternalAPISettings { Interval = 5, Count = 2 });
        var syncService = new BitcoinPriceSyncService(client.Object, cacheProvider.Object, NullLogger(), settings);

        await syncService.SyncOnceAsync();

        Assert.IsNotNull(cached);
        Assert.AreEqual(2, cached!.Count);
        Assert.AreEqual("2021-10-02", cached[0].Date);
        Assert.AreEqual(47071.0575m, cached[0].USD);
        cacheProvider.Verify(
            c => c.SetValueAsync(CacheKeys.ApiLatest, It.IsAny<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SyncOnceAsync_DoesNotCache_WhenClientReturnsNoData()
    {
        var client = new Mock<IBitcoinPriceIndexClient>();
        client.Setup(c => c.GetHistoricalAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<BitCoinPriceIndexHistoryModel>)[]);

        var cacheProvider = new Mock<ICacheProvider>();
        var settings = Options.Create(new ExternalAPISettings { Interval = 5, Count = 2 });
        var syncService = new BitcoinPriceSyncService(client.Object, cacheProvider.Object, NullLogger(), settings);

        await syncService.SyncOnceAsync();

        cacheProvider.Verify(
            c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static ILogger<BitcoinPriceSyncService> NullLogger() => new Mock<ILogger<BitcoinPriceSyncService>>().Object;
}
