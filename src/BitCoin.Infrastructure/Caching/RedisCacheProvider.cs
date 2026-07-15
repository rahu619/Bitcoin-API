using System.Text.Json;

using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;
using BitCoin.Infrastructure.Serialization;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.Infrastructure.Caching;

/// <summary>
/// The cache provider implementation, backed by the Redis instance provisioned via Aspire.
/// Values are JSON-serialized using the AOT-safe <see cref="BitcoinApiJsonSerializerContext"/>.
/// </summary>
public sealed partial class RedisCacheProvider : ICacheProvider
{
    private readonly ExternalAPISettings _apiSettings;
    private readonly ILogger<RedisCacheProvider> _logger;
    private readonly IDistributedCache _distributedCache;

    public RedisCacheProvider(
        ILogger<RedisCacheProvider> logger,
        IDistributedCache distributedCache,
        IOptions<ExternalAPISettings> apiSettings)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentNullException.ThrowIfNull(apiSettings);

        _logger = logger;
        _distributedCache = distributedCache;
        _apiSettings = apiSettings.Value;
    }

    /// <summary>
    /// Tries to get a value based on key.
    /// </summary>
    public async Task<T?> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var bytes = await _distributedCache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            Log.CacheKeyNotFound(_logger, key);
            return default;
        }

        var value = (T?)JsonSerializer.Deserialize(bytes, typeof(T), BitcoinApiJsonSerializerContext.Default);
        Log.CacheHit(_logger, key);
        return value;
    }

    /// <summary>
    /// Sets the cache key and value.
    /// </summary>
    public async Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), BitcoinApiJsonSerializerContext.Default);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(Math.Max(1, _apiSettings.Interval)));

        await _distributedCache.SetAsync(key, bytes, options, cancellationToken).ConfigureAwait(false);
        Log.CacheSet(_logger, key, bytes.Length);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Cache entry '{CacheKey}' was not found.")]
        public static partial void CacheKeyNotFound(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' was found.")]
        public static partial void CacheHit(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' set ({ByteCount} bytes).")]
        public static partial void CacheSet(ILogger logger, string cacheKey, int byteCount);
    }
}
