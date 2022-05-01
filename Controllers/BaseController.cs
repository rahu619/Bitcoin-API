using BitCoin.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BitCoin.API.Controllers
{
    /// <summary>
    /// The base controller
    /// TODO: Authentication will be implemented here
    /// </summary>
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class BaseController : ControllerBase
    {
        protected readonly ICacheProvider _cacheProvider;

        public BaseController(ICacheProvider cacheProvider)
        {
            this._cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(_cacheProvider));
        }

    }
}
