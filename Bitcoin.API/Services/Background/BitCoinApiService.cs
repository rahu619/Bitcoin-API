using BitCoin.API.Configuration;
using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{
    /// <summary>
    /// This hosted service will fetch latest historical values from the external Bitcoin API
    /// at every <see cref="ExternalAPISettings.Interval"/> seconds
    /// </summary>
    public class BitCoinApiService : BackgroundService
    {
        private readonly IHttpClientService<BitCoinPriceIndexModel> _consumerService;
        private readonly ILogger _logger;
        private readonly ICacheProvider _cacheProvider;
        private readonly ExternalAPISettings _apiSetting;
        private IEnumerable<BitCoinPriceIndexHistoryModel> _result;

        public BitCoinApiService(IHttpClientService<BitCoinPriceIndexModel> consumerService,
                                 ILogger<BitCoinApiService> logger,
                                 ICacheProvider cacheProvider, 
                                 IOptions<ExternalAPISettings> apiSetting)
        {
            this._consumerService = consumerService;
            this._logger = logger;
            this._cacheProvider = cacheProvider;
            this._apiSetting = apiSetting?.Value ?? throw new ArgumentNullException(nameof(apiSetting));

        }

        /// <summary>
        ///  Background service starting point
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Fetching data");

            stoppingToken.Register(() => _logger.LogDebug($" Bitcoin API background task is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunAsBackground();

                //delaying the next fetch operation by the configured interval value 
                await Task.Delay(_apiSetting.Interval * 1000, stoppingToken);
            }

            //disposing the http client service on shutdown
            _consumerService.Dispose();

            _logger.LogDebug($"Bitcoin API task is stopping");

        }

        /// <summary>
        /// Fetches the Bitcoin historical API details
        /// </summary>
        /// <returns></returns>
        private async Task RunAsBackground()
        {
            //Gets the result set in reverse chronological order
            _result = await _consumerService.GetContent(_apiSetting.Url.Historical)
                                            .ContinueWith(task => FilterContent(task.Result));

            _logger.LogDebug("Fetched {Count} items from Bitcoin API", _result.Count());

            //Retrieves the configured count from the resultset
            //and sets it to cache
            _cacheProvider.Set(Cache.API_LATEST, _result);

        }

        /// <summary>
        /// Maps the obtained json content to <see cref="BitCoinPriceIndexHistoryModel"/> model
        /// </summary>
        /// <param name="data"></param>
        private IEnumerable<BitCoinPriceIndexHistoryModel> FilterContent(BitCoinPriceIndexModel data)
        {
            var datePriceCollection = data?.BitCoinPriceIndexHistory;

            if (datePriceCollection is null)
            {
                _logger.LogError("No data found!");
                return default;
            }

            _logger.LogInformation("Bitcoin API task running");

            var resultSet = (from filter in datePriceCollection
                             orderby filter.Key descending
                             select new BitCoinPriceIndexHistoryModel
                             {
                                 Date = filter.Key,
                                 USD = filter.Value
                             });

            if (resultSet is null)
            {
                _logger.LogWarning("Empty Resultset returned!");
                return default;
            }

           return resultSet.Take(_apiSetting.Count);

        }
    }
}
