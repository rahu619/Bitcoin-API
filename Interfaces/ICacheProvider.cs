namespace BitCoin.API.Interfaces
{
    public interface ICacheProvider
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Reset();
    }
}
