using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Authorization;

public sealed record UserOrganizationSummary(
    Guid OrganizationId,
    string Role,
    string Status,
    bool IsActive);

public sealed record OrgContextResolution(
    Guid UserId,
    Guid OrganizationId,
    string Role,
    bool IsActiveMember,
    bool IsOrganizationActive,
    IReadOnlySet<string> Permissions);

public interface IOrgContextService
{
    Task<IReadOnlyList<UserOrganizationSummary>> ListUserOrganizationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OrgContextResolution?> ResolveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid organizationId,
        string permission,
        CancellationToken cancellationToken = default);
}

public class OrgContextService : IOrgContextService
{
    private readonly IMembershipRepository _memberships;
    private readonly IOrganizationStatusRepository _orgStatuses;

    public OrgContextService(
        IMembershipRepository memberships,
        IOrganizationStatusRepository orgStatuses)
    {
        _memberships = memberships;
        _orgStatuses = orgStatuses;
    }

    public async Task<IReadOnlyList<UserOrganizationSummary>> ListUserOrganizationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var memberships = await _memberships.ListByUserAsync(userId, cancellationToken);
        return memberships
            .Select(m => new UserOrganizationSummary(
                m.OrganizationId,
                m.Role,
                m.Status,
                m.IsActive))
            .ToList();
    }

    public async Task<OrgContextResolution?> ResolveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetAsync(userId, organizationId, cancellationToken);
        if (membership is null || !membership.IsActive)
        {
            return null;
        }

        var orgActive = await _orgStatuses.IsOrganizationActiveAsync(organizationId, cancellationToken);
        if (!orgActive)
        {
            return new OrgContextResolution(
                userId,
                organizationId,
                membership.Role,
                IsActiveMember: true,
                IsOrganizationActive: false,
                Permissions: new HashSet<string>());
        }

        var permissions = OrganizationPermissionMatrix.GetPermissions(membership.Role);
        return new OrgContextResolution(
            userId,
            organizationId,
            membership.Role,
            IsActiveMember: true,
            IsOrganizationActive: true,
            Permissions: permissions);
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid organizationId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveAsync(userId, organizationId, cancellationToken);
        return context is { IsActiveMember: true, IsOrganizationActive: true }
            && context.Permissions.Contains(permission);
    }
}