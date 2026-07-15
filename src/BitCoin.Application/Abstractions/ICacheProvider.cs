namespace BitCoin.Application.Abstractions;

/// <summary>
/// Port for the distributed cache.
/// </summary>
public interface ICacheProvider
{
    public Task<T?> TryGetValueAsync<T>(string key, CancellationToken cancellationToken = default);

    public Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default);
}
