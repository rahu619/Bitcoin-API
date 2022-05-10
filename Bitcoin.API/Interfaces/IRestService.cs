using System;
using System.Threading.Tasks;

namespace BitCoin.API.Interfaces
{
    /// <summary>
    /// Interface for the REST service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHttpClientService<T> : IDisposable
    {
        Task<T> GetContent(string url);
    }
}
