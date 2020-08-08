using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace BitCoin.API.Services
{
    public class CacheProvider : ICacheProvider
    {
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        private readonly IMemoryCache _memoryCache;
        private readonly ExternalAPISettings _apiSetting;


        public CacheProvider(IMemoryCache memoryCache, IOptions<ExternalAPISettings> apiSettings)
        {
            this._memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(_memoryCache));
            this._apiSetting = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        //public async Task<T> GetOrCreateAsync<T>(string key, T value)
        //{
        //    return await _memoryCache.GetOrCreateAsync<T>(key, entry =>
        //    {
        //        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(this._apiSetting.Interval);
        //    },);
        //}

        public T Get<T>(string key)
        {
            if (_memoryCache.TryGetValue<T>(key, out T result))
            {
                return result;
            }
            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(this._apiSetting.Interval));
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            _memoryCache.Set(key, value);
        }



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
