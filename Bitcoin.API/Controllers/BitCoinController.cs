using System.Collections.Generic;

using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitCoin.API.Controllers
{
    public class BitCoinController : BaseController
    {
        public BitCoinController(ICacheProvider cacheProvider)
            : base(cacheProvider)
        {
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
            var result = CacheProvider.Get<IReadOnlyList<BitCoinPriceIndexHistoryModel>>(CacheKeys.ApiLatest);

            if (result is null || result.Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
