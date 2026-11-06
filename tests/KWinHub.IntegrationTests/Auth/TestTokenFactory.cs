using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KWinHub.IntegrationTests.Auth;

public static class TestTokenFactory
{
    public const string TestSigningKey = "test-super-secret-key-at-least-32-chars-long!!";
    public const string TestIssuer = "kwinhub";
    public const string PlatformAudience = "kwinhub-platform-api";
    public const string HubAudience = "kwinhub-hub-api";

    public static string CreateToken(
        IEnumerable<string> audiences,
        string issuer = TestIssuer,
        DateTime? expires = null,
        string signingKey = TestSigningKey,
        Guid? userId = null,
        Guid? orgId = null,
        string role = "Owner",
        bool isPlatformAdmin = false,
        string? email = null)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? Guid.NewGuid()).ToString()),
            new("org_id", (orgId ?? Guid.NewGuid()).ToString()),
            new("org_role", role),
        };

        if (email is not null)
        {
            claims.Add(new Claim("email", email));
        }

        if (isPlatformAdmin)
        {
            claims.Add(new Claim("platform_admin", "true"));
        }

        foreach (var audience in audiences)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
        }

        var payload = new JwtPayload(
            issuer: issuer,
            audience: null,
            claims: claims,
            notBefore: null,
            expires: expires ?? DateTime.UtcNow.AddHours(1));

        var token = new JwtSecurityToken(new JwtHeader(credentials), payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreateMalformedToken() => "this-is-not-a-valid-jwt-token";

    public static string CreateTokenWithWrongKey()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrong-key-that-is-at-least-32-characters!!")),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Aud, PlatformAudience),
        };

        var payload = new JwtPayload(TestIssuer, null, claims, null, DateTime.UtcNow.AddHours(1));
        var token = new JwtSecurityToken(new JwtHeader(credentials), payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}