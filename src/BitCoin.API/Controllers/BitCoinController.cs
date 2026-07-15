using BitCoin.Application.Abstractions;
using BitCoin.Domain;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitCoin.API.Controllers;

public sealed class BitCoinController : BaseController
{
    private readonly IBitcoinPriceQueryService _queryService;

    public BitCoinController(IBitcoinPriceQueryService queryService)
    {
        ArgumentNullException.ThrowIfNull(queryService);

        _queryService = queryService;
    }

    /// <summary>
    /// Retrieves the latest Bitcoin Price Index values.
    /// </summary>
    [HttpGet]
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BitCoinPriceIndexHistoryModel>>> GetLatest(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLatestAsync(cancellationToken).ConfigureAwait(false);

        return result.Count == 0 ? NotFound() : Ok(result);
    }
}
