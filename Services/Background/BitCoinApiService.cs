using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Extension;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{
    public class BitCoinApiService : BackgroundService
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

        private async Task RunAsBackground()
        {
            //Gets the result set in reverse chronological order
            var backgroundTask = _consumerService.GetContent(_apiSetting.Url.Historical)
                                                  .ContinueWith(task => Process(task.Result));

            await backgroundTask;
        }


        private void Process(BitCoinPriceIndexModel data)
        {
            var datePriceCollection = data?.BitCoinPriceIndexHistory;

            if (datePriceCollection is null)
            {
                _logger.LogError("No data found!");
                return;
            }

            _logger.IncludeTimeStamp($"Bitcoin.Api task doing background work.");

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

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.IncludeTimeStamp("Fetching data");

            stoppingToken.Register(() =>
                _logger.LogDebug($" Bitcoin.Api background task is stopping."));

            //await RunAsBackground();

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunAsBackground();

                //keeping the interval as seconds.
                await Task.Delay(_apiSetting.Interval * 1000, stoppingToken);
            }

            _logger.LogDebug($"Bitcoin.Api background task is stopping.");

        }
    }
}
