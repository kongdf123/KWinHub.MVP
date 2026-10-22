using System.Security.Claims;
using KWinHub.Application.Abstractions;
using KWinHub.Application.Platform;
using KWinHub.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform/organizations")]
public class PlatformOrganizationsController : ControllerBase
{
    private readonly IPlatformService _platformService;
    private readonly ICurrentUserContext _currentUser;

    public PlatformOrganizationsController(
        IPlatformService platformService,
        ICurrentUserContext currentUser)
    {
        _platformService = platformService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlatformOrganizationResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var organizations = await _platformService.ListOrganizationsAsync(
            search,
            status,
            type,
            cancellationToken);

        return Ok(organizations);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlatformOrganizationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformOrganizationDetailResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await _platformService.GetOrganizationAsync(id, cancellationToken);
        return organization is null ? NotFound() : Ok(organization);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] UpdateOrganizationStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _platformService.ChangeOrganizationStatusAsync(
                id,
                request.Action,
                _currentUser.UserId,
                ActorEmail,
                IpAddress,
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status action",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private string? ActorEmail =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;

    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}
