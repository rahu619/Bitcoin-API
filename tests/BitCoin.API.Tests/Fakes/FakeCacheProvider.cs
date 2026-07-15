using System.Collections.Concurrent;

using BitCoin.Application.Abstractions;

namespace BitCoin.API.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ICacheProvider"/> used to keep controller integration tests independent
/// of a real Redis instance.
/// </summary>
internal sealed class FakeCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, object> _store = new();

    public Task<T?> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _store.TryGetValue(key, out var stored) ? (T)stored : default;
        return Task.FromResult(value);
    }

    public Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        if (value is not null)
        {
            _store[key] = value;
        }

        return Task.CompletedTask;
    }
}
