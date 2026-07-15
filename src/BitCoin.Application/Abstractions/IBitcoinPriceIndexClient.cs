using BitCoin.Domain;

namespace BitCoin.Application.Abstractions;

/// <summary>
/// Port for retrieving Bitcoin price history from an external data source.
/// </summary>
public interface IBitcoinPriceIndexClient
{
    /// <summary>
    /// Retrieves historical Bitcoin price points, ordered newest first.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    public Task<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetHistoricalAsync(CancellationToken cancellationToken = default);
}
