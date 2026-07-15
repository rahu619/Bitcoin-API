using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using BitCoin.API.Controllers;
using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using BitCoin.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BitCoin.API.Test;

[TestClass]
public class CoreTest
{
    // Unix ms timestamps for 2021-09-30, 2021-10-01, 2021-10-02 (UTC midnight), matching CoinGecko's market_chart shape.
    private const string HistoricalPayload = "{\n  \"prices\": [\n    [1632960000000, 41501.6017],\n    [1633046400000, 43823.5533],\n    [1633132800000, 47071.0575]\n  ]\n}";

    [TestMethod]
    public async Task ShouldFetchBitcoinHistoricalApiSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BitCoinApiService>>();
        var mockCacheProvider = new Mock<ICacheProvider>();

        var priceIndexClient = new StubBitcoinPriceIndexClient(HistoricalPayload);

        var externalApiSetting = new ExternalAPISettings
        {
            Interval = 5,
            Count = 2,
            Url = new Url { Base = "https://api.coingecko.com/api/v3", Historical = "/coins/bitcoin/market_chart?vs_currency=usd&days=30&interval=daily" }
        };

        var backgroundService = new BitCoinApiService(
            priceIndexClient,
            mockLogger.Object,
            mockCacheProvider.Object,
            Options.Create(externalApiSetting));

        // Act
        await backgroundService.ExecuteIterationAsync();
        var result = backgroundService.LatestResult;

        // Assert
        Assert.AreEqual(externalApiSetting.Count, result.Count);
        Assert.AreEqual("2021-10-02", result[0].Date);
        Assert.AreEqual(47071.0575m, result[0].USD);

        mockCacheProvider.Verify(
            cache => cache.SetValue(CacheKeys.ApiLatest, It.Is<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(list => list == result)),
            Times.Once);
    }

    [TestMethod]
    public void ShouldRequireAuthorizationForApiControllers()
    {
        var authorizeAttribute = typeof(BaseController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.IsNotNull(authorizeAttribute);
    }

    private sealed class StubBitcoinPriceIndexClient : IBitcoinPriceIndexClient
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly BitCoinPriceIndexModel? _payload;

        public StubBitcoinPriceIndexClient(string payload)
        {
            _payload = JsonSerializer.Deserialize<BitCoinPriceIndexModel>(payload, SerializerOptions);
        }

        public Task<BitCoinPriceIndexModel?> GetHistoricalAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_payload);
    }
}
