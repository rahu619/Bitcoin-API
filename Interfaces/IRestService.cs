using System.Threading.Tasks;

namespace BitCoin.API.Interfaces
{
    public interface IRestService<T>
    {
        Task<T> GetContent(string url);
    }
}
