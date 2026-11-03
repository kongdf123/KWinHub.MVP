using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Users;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Platform.Api.InternalApi;

[ApiController]
[InternalApiKey]
[Route("internal/v1/users")]
public class InternalUsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public InternalUsersController(IUserRepository users)
    {
        _users = users;
    }

    [HttpGet("{userId:guid}/security-stamp")]
    public async Task<IActionResult> ValidateSecurityStamp(
        Guid userId,
        [FromQuery] string stamp,
        CancellationToken cancellationToken)
    {
        var current = await _users.GetSecurityStampAsync(userId, cancellationToken);
        if (current is null)
        {
            return NotFound();
        }

        return Ok(new { valid = string.Equals(current, stamp, StringComparison.Ordinal) });
    }
}