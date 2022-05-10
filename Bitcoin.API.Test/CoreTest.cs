using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using BitCoin.API.Services;
using BitCoin.API.Test.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BitCoin.API.Test
{
    [TestClass]
    public class CoreTest
    {
        private const string BASE_URL = "https://api.coindesk.com/v1/bpi";
        private const string HISTORICAL_URL = "/historical/close.json";

        [TestMethod]
        public async Task Should_Fetch_BitCoin_HISTORICAL_API_Successfully()
        {
            //Arrange
            var mockLogger = new Mock<ILogger<BitCoinApiService>>();
            var mockCacheProvider = new Mock<ICacheProvider>();
            var httpClientService = new HttpClientService<BitCoinPriceIndexModel>();
            var externalAPISetting = new ExternalAPISettings { Url = new Url { Base = BASE_URL, Historical = HISTORICAL_URL }, Count = 5 };

            var backgroundService = new BitCoinApiService(httpClientService, mockLogger.Object, mockCacheProvider.Object, Options.Create(externalAPISetting));

            //Act
            await backgroundService.InvokePrivateMethod<Task, BitCoinApiService>("RunAsBackground", null);

            var result = backgroundService.InvokePrivateField<IEnumerable<BitCoinPriceIndexHistoryModel>, BitCoinApiService>("_result");

            //Assert
            Assert.IsTrue(result.Any());
            Assert.AreEqual(externalAPISetting.Count, result.Count());
        }
    }
}
