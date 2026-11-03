using System.Security.Claims;
using KWinHub.Platform.Application.Registration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace KWinHub.Platform.Api.Authorization;

/// <summary>
/// Validates the <c>sec_stamp</c> claim of an incoming JWT against the
/// authoritative value stored in the Platform database. When the stamp no
/// longer matches (session revoked, password changed) the request is rejected
/// with 401 so revoked tokens cannot be used for the remainder of their TTL.
/// Can be disabled via <c>Jwt:ValidateSecurityStamp = false</c> for test
/// environments where the user table is not pre-seeded.
/// </summary>
public static class SecurityStampValidation
{
    public static JwtBearerEvents Events { get; } = new()
    {
        OnTokenValidated = async context =>
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            if (!configuration.GetValue("Jwt:ValidateSecurityStamp", true))
            {
                return;
            }

            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)
                ?? context.Principal?.FindFirst("sub");

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Fail("missing_user_id");
                return;
            }

            var stampClaim = context.Principal?.FindFirst("sec_stamp");
            if (stampClaim is null || string.IsNullOrWhiteSpace(stampClaim.Value))
            {
                context.Fail("missing_security_stamp");
                return;
            }

            var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var currentStamp = await users.GetSecurityStampAsync(userId, context.HttpContext.RequestAborted);

            if (currentStamp is null
                || !string.Equals(currentStamp, stampClaim.Value, StringComparison.Ordinal))
            {
                context.Fail("security_stamp_mismatch");
            }
        }
    };
}