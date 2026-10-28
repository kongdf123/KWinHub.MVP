using KWinHub.Application.Subscriptions;

namespace KWinHub.Application.Authorization;

public class OrganizationAccessService : IOrganizationAccessService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEntitlementResolver _entitlementResolver;

    public OrganizationAccessService(
        IMembershipRepository membershipRepository,
        IEntitlementResolver entitlementResolver)
    {
        _membershipRepository = membershipRepository;
        _entitlementResolver = entitlementResolver;
    }

    public async Task<AccessDecision> AuthorizeAsync(
        Guid organizationId,
        Guid userId,
        string featureKey,
        string? permissionKey = null,
        string? resourceType = null,
        Guid? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        // 1) Membership / tenant scope.
        bool isMember = await _membershipRepository.IsMemberAsync(userId, organizationId, cancellationToken);
        if (!isMember)
        {
            return new AccessDecision(false, "not_a_member");
        }

        // 2) Feature entitlement (fail closed on resolver errors).
        try
        {
            var entitlement = await _entitlementResolver.ResolveAsync(organizationId, featureKey, cancellationToken);
            if (!entitlement.Enabled)
            {
                return new AccessDecision(false, "feature_not_entitled");
            }
        }
        catch
        {
            // Fail closed for gated writes when entitlement store is unavailable.
            return new AccessDecision(false, "entitlement_unavailable");
        }

        // 3) Permission + resource scope checks are deferred to dedicated policies
        // in later phases; V1 enforces membership + feature entitlement.
        return new AccessDecision(true, null);
    }
}
