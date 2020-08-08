using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using DNTScheduler.Core.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{

    /// <summary>
    /// In an ideal world, this could be a windows service application of it's own.
    /// </summary>
    public class BitCoinApiService : IScheduledTask, IHostedService
    {
        private readonly IRestService<BitCoinPriceIndexModel> _consumerService;
        private readonly ILogger<BitCoinApiService> _logger;
        private readonly ICacheProvider _cacheProvider;
        private readonly ExternalAPISettings _apiSetting;

        public BitCoinApiService(IRestService<BitCoinPriceIndexModel> consumerService, ILogger<BitCoinApiService> logger, ICacheProvider cacheProvider, IOptions<ExternalAPISettings> apiSetting)
        {
            this._consumerService = consumerService;
            this._logger = logger;
            this._cacheProvider = cacheProvider;
            this._apiSetting = apiSetting?.Value ?? throw new ArgumentNullException(nameof(apiSetting));

        }

        //To retrieve all the results before actually receiving requests
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


        public bool IsShuttingDown { get; set; }

        public async Task RunAsync()
        {
            if (this.IsShuttingDown)
            {
                return;
            }

            _logger.LogInformation("Fetching data from API : ", DateTime.Now.ToUniversalTime());

            //Gets the result set in reverse chronological order
            var result = await _consumerService.GetContent(_apiSetting.Url.Historical);

            Process(result);

            //await Task.Delay(TimeSpan.FromSeconds(30));
        }



        private void Process(BitCoinPriceIndexModel data)
        {
            var datePriceCollection = data?.BitCoinPriceIndexHistory;

            if (datePriceCollection is null)
            {
                _logger.LogError("No data found!");
                return;
            }

            var resultSet = (from filter in datePriceCollection
                             orderby filter.Key descending
                             select new BitCoinPriceIndexHistoryModel
                             {
                                 Date = filter.Key,
                                 USD = filter.Value
                             });

            if (resultSet is null)
                return;

            _cacheProvider.Set(Cache.API_LATEST, resultSet.Take(5));

        }
    }
}
