using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KWinHub.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace KWinHub.Api.Services;

public class JwtTokenIssuer : IJwtTokenIssuer
{
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
        bool isPlatformAdmin)
    {
        var key = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new("email", email),
            new("name", displayName),
            new("org_id", organizationId.ToString()),
            new("org_role", organizationRole),
        };

        if (isPlatformAdmin)
        {
            claims.Add(new Claim("platform_admin", "true"));
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "kwinhub";
        var audience = _configuration["Jwt:Audience"] ?? "kwinhub-api";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        _logger.LogInformation("JWT issued for User:{UserId} Org:{OrgId} Role:{Role} PlatformAdmin:{IsPlatformAdmin}",
            userId, organizationId, organizationRole, isPlatformAdmin);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
