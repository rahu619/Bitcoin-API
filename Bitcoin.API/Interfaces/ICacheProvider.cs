namespace BitCoin.API.Interfaces
{
    /// <summary>
    /// Interface for the cache provider.
    /// </summary>
    public interface ICacheProvider
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Reset();
    }
}
