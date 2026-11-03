using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KWinHub.Platform.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace KWinHub.Platform.Api.Services;

/// <summary>
/// Issues the M0 local JWT. A login token carries BOTH API audiences so the same
/// identity can call the Platform API and the Hub API, while each host only
/// accepts its own audience. M1 replaces this with the OIDC authority (ADR-0001).
/// </summary>
public class JwtTokenIssuer : IJwtTokenIssuer
{
    public const string PlatformAudience = "kwinhub-platform-api";
    public const string HubAudience = "kwinhub-hub-api";

    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenIssuer> _logger;

    public JwtTokenIssuer(IConfiguration configuration, ILogger<JwtTokenIssuer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string IssueToken(
        Guid userId,
        string email,
        string displayName,
        Guid organizationId,
        string organizationRole,
        bool isPlatformAdmin,
        string securityStamp)
    {
        var key = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["Jwt:Issuer"] ?? "kwinhub";
        var audiences = new[]
        {
            _configuration["Jwt:PlatformAudience"] ?? PlatformAudience,
            _configuration["Jwt:HubAudience"] ?? HubAudience
        };

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("email", email),
            new("name", displayName),
            new("org_id", organizationId.ToString()),
            new("org_role", organizationRole),
            new("sec_stamp", securityStamp),
        };

        foreach (var audience in audiences)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
        }

        if (isPlatformAdmin)
        {
            claims.Add(new Claim("platform_admin", "true"));
        }

        // Passing the aud claims into JwtPayload makes it serialize "aud" as an
        // array, so both APIs accept the token against their own valid audience.
        var payload = new JwtPayload(
            issuer: issuer,
            audience: null,
            claims: claims,
            notBefore: null,
            expires: DateTime.UtcNow.AddHours(8));

        var token = new JwtSecurityToken(new JwtHeader(credentials), payload);

        _logger.LogInformation(
            "JWT issued for User:{UserId} Org:{OrgId} Role:{Role} PlatformAdmin:{IsPlatformAdmin}",
            userId, organizationId, organizationRole, isPlatformAdmin);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}