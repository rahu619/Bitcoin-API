using BitCoin.Application.Abstractions;
using BitCoin.Application.Configuration;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitCoin.Infrastructure.Caching;

/// <summary>
/// The cache provider implementation, backed by <see cref="HybridCache"/>: an in-process L1 in
/// front of the Redis L2 provisioned via Aspire. The L1 tier means concurrent reads for the same
/// key are served from memory and coalesced instead of each issuing its own Redis round-trip.
/// </summary>
public sealed partial class CacheProvider : ICacheProvider
{
    private readonly ExternalAPISettings _apiSettings;
    private readonly ILogger<CacheProvider> _logger;
    private readonly HybridCache _cache;

    public CacheProvider(
        ILogger<CacheProvider> logger,
        HybridCache cache,
        IOptions<ExternalAPISettings> apiSettings)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(apiSettings);

        _logger = logger;
        _cache = cache;
        _apiSettings = apiSettings.Value;
    }

    /// <summary>
    /// Tries to get a value based on key. Never fetches on a miss - just reports whether the
    /// value that <see cref="SetValueAsync{T}"/> last wrote for this key is still cached.
    /// </summary>
    public async Task<T?> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = await _cache
            .GetOrCreateAsync(key, static _ => ValueTask.FromResult(default(T)!), GetEntryOptions(), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (value is null)
        {
            Log.CacheKeyNotFound(_logger, key);
            return default;
        }

        Log.CacheHit(_logger, key);
        return value;
    }

    /// <summary>
    /// Sets the cache key and value.
    /// </summary>
    public async Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _cache.SetAsync(key, value, GetEntryOptions(), cancellationToken: cancellationToken).ConfigureAwait(false);
        Log.CacheSet(_logger, key);
    }

    private HybridCacheEntryOptions GetEntryOptions()
    {
        var distributedTtl = TimeSpan.FromSeconds(Math.Max(1, _apiSettings.Interval));
        var localTtl = distributedTtl < TimeSpan.FromSeconds(2) ? distributedTtl : TimeSpan.FromSeconds(2);

        return new HybridCacheEntryOptions
        {
            Expiration = distributedTtl,
            LocalCacheExpiration = localTtl
        };
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Cache entry '{CacheKey}' was not found.")]
        public static partial void CacheKeyNotFound(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' was found.")]
        public static partial void CacheHit(ILogger logger, string cacheKey);

        [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "Cache entry '{CacheKey}' set.")]
        public static partial void CacheSet(ILogger logger, string cacheKey);
    }
}
