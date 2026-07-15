using BitCoin.Application.Abstractions;
using BitCoin.Application.Constants;
using BitCoin.Application.Services;
using BitCoin.Domain;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace BitCoin.Application.Tests;

[TestClass]
public sealed class BitcoinPriceQueryServiceTests
{
    [TestMethod]
    public async Task GetLatestAsync_ReturnsCachedValues_WhenCacheHasData()
    {
        IReadOnlyList<BitCoinPriceIndexHistoryModel> cached = [new() { Date = "2026-07-14", USD = 65000m }];

        var cacheProvider = new Mock<ICacheProvider>();
        cacheProvider
            .Setup(c => c.TryGetValueAsync<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(CacheKeys.ApiLatest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var queryService = new BitcoinPriceQueryService(cacheProvider.Object);

        var result = await queryService.GetLatestAsync();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(65000m, result[0].USD);
    }

    [TestMethod]
    public async Task GetLatestAsync_ReturnsEmpty_WhenCacheIsEmpty()
    {
        var cacheProvider = new Mock<ICacheProvider>();
        cacheProvider
            .Setup(c => c.TryGetValueAsync<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(CacheKeys.ApiLatest, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<BitCoinPriceIndexHistoryModel>?)null);

        var queryService = new BitcoinPriceQueryService(cacheProvider.Object);

        var result = await queryService.GetLatestAsync();

        Assert.AreEqual(0, result.Count);
    }
}
