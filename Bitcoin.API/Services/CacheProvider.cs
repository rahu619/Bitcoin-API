using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace BitCoin.API.Services
{
    /// <summary>
    /// The cache provider implementation.
    /// The current version uses in-memory caching
    /// </summary>
    public class InMemoryCacheProvider : ICacheProvider
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ExternalAPISettings _apiSetting;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        //TODO: To limit the number of threads accessing the cache - in case of a lock event
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1);

        public InMemoryCacheProvider(ILogger<InMemoryCacheProvider> logger, IMemoryCache memoryCache, IOptions<ExternalAPISettings> apiSettings)
        {
            this._logger = logger;
            this._memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(_memoryCache));
            this._apiSetting = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        /// <summary>
        /// Gets the value based on key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (_memoryCache.TryGetValue<T>(key, out T result))
            {
                return result;
            }
            this._logger.LogError("Cache [{key}] not found!", key);

            return default(T);
        }

        /// <summary>
        /// Sets the cache key and value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(this._apiSetting.Interval));
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            _memoryCache.Set(key, value);
        }

        /// <summary>
        /// Cleans up the cache
        /// </summary>
        public void Reset()
        {
            if (_memoryCache == null || _resetCacheToken.IsCancellationRequested || !_resetCacheToken.Token.CanBeCanceled)
            {
                return;
            }
            _resetCacheToken.Cancel();
            _resetCacheToken.Dispose();

            _resetCacheToken = new CancellationTokenSource();
        }
    }
}
