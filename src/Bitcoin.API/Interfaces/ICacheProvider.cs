namespace BitCoin.API.Interfaces;

/// <summary>
/// Interface for the cache provider.
/// </summary>
public interface ICacheProvider
{
    bool TryGetValue<T>(string key, out T? value);
    void SetValue<T>(string key, T value);
    void Reset();
}
