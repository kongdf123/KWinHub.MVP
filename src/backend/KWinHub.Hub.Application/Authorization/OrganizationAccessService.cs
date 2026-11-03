using KWinHub.Hub.Application.CrossContext;

namespace KWinHub.Hub.Application.Authorization;

public class OrganizationAccessService : IOrganizationAccessService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlatformGateway _platformGateway;

    public OrganizationAccessService(
        IMembershipRepository membershipRepository,
        IPlatformGateway platformGateway)
    {
        _membershipRepository = membershipRepository;
        _platformGateway = platformGateway;
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

        // 2) Feature entitlement (fail closed when Platform is unreachable).
        try
        {
            var decision = await _platformGateway.ResolveEntitlementAsync(
                organizationId,
                featureKey,
                cancellationToken);

            if (!decision.Allowed)
            {
                return new AccessDecision(false, "feature_not_entitled");
            }
        }
        catch
        {
            // Fail closed for gated writes when the entitlement authority is unavailable.
            return new AccessDecision(false, "entitlement_unavailable");
        }

        // 3) Permission + resource scope checks are deferred to dedicated policies
        // in later phases; V1 enforces membership + feature entitlement.
        return new AccessDecision(true, null);
    }
}
