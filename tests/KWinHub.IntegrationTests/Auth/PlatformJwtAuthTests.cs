using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KWinHub.IntegrationTests.Auth;

public class PlatformJwtAuthTests : IClassFixture<PlatformApiFactory>
{
    private readonly PlatformApiFactory _factory;

    public PlatformJwtAuthTests(PlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NoToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CorrectAudienceToken_Accepted()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(new[] { TestTokenFactory.PlatformAudience });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongAudienceToken_Rejected()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(new[] { TestTokenFactory.HubAudience });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongIssuerToken_Rejected()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.PlatformAudience },
            issuer: "evil-issuer");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_Rejected()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.PlatformAudience },
            expires: DateTime.UtcNow.AddMinutes(-5));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MalformedToken_Rejected()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenFactory.CreateMalformedToken());

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongSigningKeyToken_Rejected()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenFactory.CreateTokenWithWrongKey());

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DevTokenEndpoint_ReturnsForbidden_InProduction()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/devauth/token",
            new { UserId = Guid.NewGuid(), OrganizationId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}