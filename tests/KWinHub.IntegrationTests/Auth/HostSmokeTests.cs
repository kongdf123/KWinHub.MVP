using System.Net;
using System.Net.Http.Headers;

namespace KWinHub.IntegrationTests.Auth;

public class PlatformHostSmokeTests : IClassFixture<PlatformApiFactory>
{
    private readonly PlatformApiFactory _factory;

    public PlatformHostSmokeTests(PlatformApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_DoesNotReturn401()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(new[] { TestTokenFactory.PlatformAudience });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/me");
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class PlatformSwaggerTests : IClassFixture<PlatformApiDevFactory>
{
    private readonly PlatformApiDevFactory _factory;

    public PlatformSwaggerTests(PlatformApiDevFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("KWinHub Platform API", content);
    }
}

public class HubHostSmokeTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public HubHostSmokeTests(HubApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/subjects");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubjectsList_WithValidToken_DoesNotReturn401()
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(new[] { TestTokenFactory.HubAudience });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/subjects");
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class HubSwaggerTests : IClassFixture<HubApiDevFactory>
{
    private readonly HubApiDevFactory _factory;

    public HubSwaggerTests(HubApiDevFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("KWinHub Hub API", content);
    }
}