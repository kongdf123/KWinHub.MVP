using System.Security.Claims;
using KWinHub.Application.Abstractions;

namespace KWinHub.Api.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = FindFirst(ClaimTypes.NameIdentifier) ?? FindFirst("sub");
            return claim is not null && Guid.TryParse(claim.Value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public Guid OrganizationId
    {
        get
        {
            var claim = FindFirst("org_id");
            return claim is not null && Guid.TryParse(claim.Value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string? OrganizationRole => FindFirst("org_role")?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private Claim? FindFirst(string type) =>
        _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == type);
}