using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using BitCoin.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BitCoin.API.Test;

[TestClass]
public class CoreTest
{
    private const string HistoricalPayload = "{\n  \"bpi\": {\n    \"2021-10-01\": 43823.5533,\n    \"2021-10-02\": 47071.0575,\n    \"2021-09-30\": 41501.6017\n  },\n  \"disclaimer\": \"For testing only\",\n  \"time\": {\n    \"updated\": \"Oct 3, 2021\",\n    \"updatedISO\": \"2021-10-03T00:03:00+00:00\"\n  }\n}";

    [TestMethod]
    public async Task Should_Fetch_BitCoin_Historical_Api_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BitCoinApiService>>();
        var mockCacheProvider = new Mock<ICacheProvider>();

        var priceIndexClient = new StubBitcoinPriceIndexClient(HistoricalPayload);

        var externalApiSetting = new ExternalAPISettings
        {
            Interval = 5,
            Count = 2,
            Url = new Url { Base = "https://api.coindesk.com/v1/bpi", Historical = "/historical/close.json" }
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
            cache => cache.Set(CacheKeys.ApiLatest, It.Is<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(list => list == result)),
            Times.Once);
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
