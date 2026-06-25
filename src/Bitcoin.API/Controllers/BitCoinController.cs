using System.Collections.Generic;

using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitCoin.API.Controllers;

public sealed class BitCoinController : BaseController
{
    private readonly IBitcoinPriceQueryService _queryService;

    public BitCoinController(IBitcoinPriceQueryService queryService)
    {
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    /// <summary>
    /// Retrieves the latest Bitcoin Price Index values.
    /// </summary>
    [HttpGet]
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<BitCoinPriceIndexHistoryModel>> GetLatest()
    {
        if (!_queryService.TryGetLatest(out var result))
        {
            return NotFound();
        }

        return Ok(result);
    }
}
