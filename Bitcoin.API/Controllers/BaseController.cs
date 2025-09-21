using BitCoin.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitCoin.API.Controllers;

/// <summary>
/// Provides common functionality for API controllers.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected BaseController(ICacheProvider cacheProvider)
    {
        ArgumentNullException.ThrowIfNull(cacheProvider);
        CacheProvider = cacheProvider;
    }

    /// <summary>
    /// Gets the cache provider associated with the controller.
    /// </summary>
    protected ICacheProvider CacheProvider { get; }
}
