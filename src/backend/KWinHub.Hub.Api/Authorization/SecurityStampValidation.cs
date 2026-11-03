using System.Security.Claims;
using KWinHub.Hub.Application.CrossContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace KWinHub.Hub.Api.Authorization;

/// <summary>
/// Validates the <c>sec_stamp</c> claim of an incoming JWT against the
/// authoritative value held by the Platform. The Hub never owns user
/// credentials, so it delegates the revocation check to the Platform gateway.
/// Fail closed: any transport or service error results in a 401.
/// Can be disabled via <c>Jwt:ValidateSecurityStamp = false</c> for test
/// environments where the Platform gateway is not wired up.
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

            try
            {
                var gateway = context.HttpContext.RequestServices.GetRequiredService<IPlatformGateway>();
                var valid = await gateway.ValidateSecurityStampAsync(
                    userId,
                    stampClaim.Value,
                    context.HttpContext.RequestAborted);

                if (!valid)
                {
                    context.Fail("security_stamp_mismatch");
                }
            }
            catch
            {
                context.Fail("security_stamp_validation_failed");
            }
        }
    };
}