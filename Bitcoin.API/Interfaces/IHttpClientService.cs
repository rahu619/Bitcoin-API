using System.Threading;
using System.Threading.Tasks;

namespace BitCoin.API.Interfaces;

/// <summary>
/// Interface for the REST service.
/// </summary>
/// <typeparam name="T">Type of the expected response payload.</typeparam>
public interface IHttpClientService<T>
{
    Task<T> GetContentAsync(string url, CancellationToken cancellationToken = default);
}
