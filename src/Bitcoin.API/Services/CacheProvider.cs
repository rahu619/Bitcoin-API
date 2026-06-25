using System;
using System.Threading;

using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace BitCoin.API.Services;

/// <summary>
/// The cache provider implementation.
/// The current version uses in-memory caching
/// </summary>
public sealed partial class InMemoryCacheProvider : ICacheProvider, IDisposable
{
    private CancellationTokenSource _resetCacheToken = new();

    private readonly ExternalAPISettings _apiSetting;
    private readonly ILogger<InMemoryCacheProvider> _logger;
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheProvider(
        ILogger<InMemoryCacheProvider> logger,
        IMemoryCache memoryCache,
        IOptions<ExternalAPISettings> apiSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _apiSetting = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
    }

    /// <summary>
    /// Tries to get a value based on key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            value = cachedValue;
            return true;
        }

        value = default;
        Log.CacheKeyNotFound(_logger, key);
        return false;
    }

    /// <summary>
    /// Sets the cache key and value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(Math.Max(1, _apiSetting.Interval)))
            .AddExpirationToken(new CancellationChangeToken(Volatile.Read(ref _resetCacheToken).Token));

        _memoryCache.Set(key, value, options);
    }

    /// <summary>
    /// Cleans up the cache
    /// </summary>
    public void Reset()
    {
        var previousToken = Interlocked.Exchange(ref _resetCacheToken, new CancellationTokenSource());
        if (previousToken is null)
        {
            return;
        }

        using (previousToken)
        {
            if (!previousToken.IsCancellationRequested)
            {
                previousToken.Cancel();
            }
        }
    }

    public void Dispose()
    {
        _resetCacheToken.Dispose();
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Cache entry '{CacheKey}' was not found.")]
        public static partial void CacheKeyNotFound(ILogger logger, string cacheKey);
    }
}
