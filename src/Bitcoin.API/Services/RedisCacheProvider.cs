using System;
using System.Collections.Concurrent;
using System.Text.Json;

using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.API.Services;

/// <summary>
/// The cache provider implementation, backed by the Redis instance provisioned via Aspire.
/// Values are JSON-serialized using the AOT-safe <see cref="BitcoinApiJsonSerializerContext"/>.
/// </summary>
public sealed partial class RedisCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, byte> _trackedKeys = new();

    private readonly ExternalAPISettings _apiSetting;
    private readonly ILogger<RedisCacheProvider> _logger;
    private readonly IDistributedCache _distributedCache;

    public RedisCacheProvider(
        ILogger<RedisCacheProvider> logger,
        IDistributedCache distributedCache,
        IOptions<ExternalAPISettings> apiSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _apiSetting = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
    }

    /// <summary>
    /// Tries to get a value based on key.
    /// </summary>
    public bool TryGetValue<T>(string key, out T? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var bytes = _distributedCache.Get(key);
        if (bytes is null)
        {
            value = default;
            Log.CacheKeyNotFound(_logger, key);
            return false;
        }

        value = (T?)JsonSerializer.Deserialize(bytes, typeof(T), BitcoinApiJsonSerializerContext.Default);
        Log.CacheHit(_logger, key);
        return value is not null;
    }

    /// <summary>
    /// Sets the cache key and value.
    /// </summary>
    public void SetValue<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), BitcoinApiJsonSerializerContext.Default);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(Math.Max(1, _apiSetting.Interval)));

        _distributedCache.Set(key, bytes, options);
        _trackedKeys.TryAdd(key, 0);
        Log.CacheSet(_logger, key, bytes.Length);
    }

    /// <summary>
    /// Removes every key this provider instance has written.
    /// </summary>
    public void Reset()
    {
        foreach (var key in _trackedKeys.Keys)
        {
            _distributedCache.Remove(key);
        }

        _trackedKeys.Clear();
        Log.CacheReset(_logger);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Cache entry '{CacheKey}' was not found.")]
        public static partial void CacheKeyNotFound(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' was found.")]
        public static partial void CacheHit(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' set ({ByteCount} bytes).")]
        public static partial void CacheSet(ILogger logger, string cacheKey, int byteCount);

        [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Cache reset; all tracked entries removed.")]
        public static partial void CacheReset(ILogger logger);
    }
}
