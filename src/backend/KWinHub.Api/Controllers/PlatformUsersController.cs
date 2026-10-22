using System.Security.Claims;
using KWinHub.Application.Abstractions;
using KWinHub.Application.Platform;
using KWinHub.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform/users")]
public class PlatformUsersController : ControllerBase
{
    private readonly IPlatformService _platformService;
    private readonly ICurrentUserContext _currentUser;

    public PlatformUsersController(
        IPlatformService platformService,
        ICurrentUserContext currentUser)
    {
        _platformService = platformService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlatformUserResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var users = await _platformService.ListUsersAsync(search, status, cancellationToken);
        return Ok(users);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _platformService.ChangeUserStatusAsync(
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
