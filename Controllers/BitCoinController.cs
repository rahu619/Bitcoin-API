using BitCoin.API.Constants;
using BitCoin.API.Interfaces;
using BitCoin.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitCoin.API.Controllers
{
    public class BitCoinController : BaseController
    {
        public BitCoinController(ICacheProvider cacheProvider) : base(cacheProvider) { }


        /// <summary>
        /// Gets all value from the Bitcoin Price Index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<IActionResult> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the latest Bitcoin Price Index values. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("latest")]
        [Produces(typeof(IEnumerable<BitCoinPriceIndexHistoryModel>))]
        public IActionResult GetLatest()
        {
            var result = base._cacheProvider.Get<IEnumerable<BitCoinPriceIndexHistoryModel>>(Cache.API_LATEST);
            if (result is null) {
                return NotFound();
            }

            return Ok(result);
        }


    }
}
