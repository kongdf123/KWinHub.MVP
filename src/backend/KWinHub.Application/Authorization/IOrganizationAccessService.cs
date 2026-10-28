namespace KWinHub.Application.Authorization;

public sealed record AccessDecision(bool Allowed, string? DenialCode);

/// <summary>
/// Authoritative authorization for organization-scoped operations. Combines
/// membership validation, feature entitlement, permission and resource scope.
/// Fail closed for gated writes when the entitlement store is unavailable.
/// </summary>
public interface IOrganizationAccessService
{
    Task<AccessDecision> AuthorizeAsync(
        Guid organizationId,
        Guid userId,
        string featureKey,
        string? permissionKey = null,
        string? resourceType = null,
        Guid? resourceId = null,
        CancellationToken cancellationToken = default);
}
