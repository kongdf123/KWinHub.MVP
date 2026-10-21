using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KWinHub.Api.Services;

public class DevTokenIssuer
{
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

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new("org_id", organizationId.ToString()),
            new("org_role", role),
        };

        var issuer = _configuration["Jwt:Issuer"] ?? "kwinhub-dev";
        var audience = _configuration["Jwt:Audience"] ?? "kwinhub-api";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        _logger.LogWarning("DEV TOKEN ISSUED for User:{UserId} Org:{OrgId} Role:{Role} — DO NOT USE IN PRODUCTION",
            userId, organizationId, role);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}