using KWinHub.Application.Platform;
using KWinHub.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform/dashboard")]
public class PlatformDashboardController : ControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformDashboardController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlatformDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformDashboardResponse>> Get(CancellationToken cancellationToken)
    {
        var dashboard = await _platformService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }
}
