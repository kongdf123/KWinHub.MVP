using KWinHub.Application.Platform;
using KWinHub.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform/audit-logs")]
public class PlatformAuditLogsController : ControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformAuditLogsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogResponse>>> List(
        [FromQuery] string? entityType,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _platformService.ListAuditLogsAsync(entityType, take, cancellationToken);
        return Ok(logs);
    }
}
