using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KWinHub.Hub.Api.Services;

public class DevTokenIssuer
{
    private const string PlatformAudience = "kwinhub-platform-api";
    private const string HubAudience = "kwinhub-hub-api";

    private readonly IConfiguration _configuration;
    private readonly ILogger<DevTokenIssuer> _logger;

    public DevTokenIssuer(IConfiguration configuration, ILogger<DevTokenIssuer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string IssueToken(Guid userId, Guid organizationId, string role = "Owner")
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
            new("org_id", organizationId.ToString()),
            new("org_role", role),
        };
        foreach (var audience in audiences)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
        }

        var payload = new JwtPayload(issuer, null, claims, null, DateTime.UtcNow.AddHours(8));
        var token = new JwtSecurityToken(new JwtHeader(credentials), payload);

        _logger.LogWarning(
            "DEV TOKEN ISSUED for User:{UserId} Org:{OrgId} Role:{Role} — DO NOT USE IN PRODUCTION",
            userId, organizationId, role);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
