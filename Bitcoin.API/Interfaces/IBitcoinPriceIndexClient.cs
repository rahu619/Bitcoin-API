using System.Threading;
using System.Threading.Tasks;

using BitCoin.API.Models;

namespace BitCoin.API.Interfaces;

/// <summary>
/// Client abstraction for retrieving Bitcoin price index data from the external API.
/// </summary>
public interface IBitcoinPriceIndexClient
{
    /// <summary>
    /// Retrieves the latest historical Bitcoin price index payload.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    Task<BitCoinPriceIndexModel?> GetHistoricalAsync(CancellationToken cancellationToken = default);
}
