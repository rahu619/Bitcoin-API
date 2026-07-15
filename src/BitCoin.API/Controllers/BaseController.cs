using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitCoin.API.Controllers;

/// <summary>
/// Provides common functionality for API controllers.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase;
