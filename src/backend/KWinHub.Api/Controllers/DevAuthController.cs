using KWinHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevAuthController : ControllerBase
{
    private readonly DevTokenIssuer _issuer;

    public DevAuthController(DevTokenIssuer issuer)
    {
        _issuer = issuer;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult IssueToken([FromBody] DevTokenRequest request)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return Problem(
                title: "Dev tokens are only available in development.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        var token = _issuer.IssueToken(request.UserId, request.OrganizationId, request.Role);
        return Ok(new { Token = token, Type = "Bearer" });
    }
}

public record DevTokenRequest(
    Guid UserId,
    Guid OrganizationId,
    string Role = "Owner");