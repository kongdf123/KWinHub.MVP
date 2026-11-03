using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Platform.Application.Abstractions;
using KWinHub.Platform.Application.Audit;
using KWinHub.Platform.Application.Authentication;
using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Platform.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IRegistrationService _registrationService;
    private readonly IJwtTokenIssuer _tokenIssuer;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUserRepository _users;
    private readonly IAuditService _audit;

    public AuthController(
        IAuthenticationService authenticationService,
        IRegistrationService registrationService,
        IJwtTokenIssuer tokenIssuer,
        ICurrentUserContext currentUser,
        IUserRepository users,
        IAuditService audit)
    {
        _authenticationService = authenticationService;
        _registrationService = registrationService;
        _tokenIssuer = tokenIssuer;
        _currentUser = currentUser;
        _users = users;
        _audit = audit;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _authenticationService.AuthenticateAsync(
            request.Email,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Detail = "The email or password is incorrect.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var token = _tokenIssuer.IssueToken(
            user.UserId,
            user.Email,
            user.DisplayName,
            user.OrganizationId,
            user.OrganizationRole,
            user.IsPlatformAdmin,
            user.SecurityStamp);

        return Ok(new LoginResponse(
            Token: token,
            Type: "Bearer",
            UserId: user.UserId,
            Email: user.Email,
            DisplayName: user.DisplayName,
            OrganizationId: user.OrganizationId,
            OrganizationName: user.OrganizationName,
            OrganizationRole: user.OrganizationRole,
            IsPlatformAdmin: user.IsPlatformAdmin,
            EmailVerified: user.EmailVerified,
            Status: user.Status));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyEmailResponse>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.VerifyEmailAsync(request.Token, cancellationToken);

        if (result is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid verification token",
                Detail = "The verification token is invalid or has expired.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result);
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await _registrationService.ResendVerificationAsync(request.Email, cancellationToken);
        return Ok(new { sent = true });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthenticatedUser>> Me(CancellationToken cancellationToken)
    {
        var profile = await _authenticationService.GetProfileAsync(_currentUser.UserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user is not null)
        {
            user.RevokeSessions();
            await _users.UpdateAsync(user, cancellationToken);

            await _audit.RecordAsync(
                "user.logout",
                "User",
                user.Id.ToString(),
                $"User logged out: {user.Email}",
                actorUserId: user.Id,
                actorEmail: user.Email,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken: cancellationToken);
        }

        return Ok(new { revoked = true });
    }
}

public record ResendVerificationRequest(string Email);