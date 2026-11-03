using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Authorization;

public interface IMembershipService
{
    /// <summary>
    /// Creates an active membership for an organization lifecycle event emitted
    /// by the Platform context. Idempotent: an existing membership is left intact.
    /// </summary>
    Task ProvisionAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<OrganizationMembership?> GetPrimaryByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

public class MembershipService : IMembershipService
{
    private const string DefaultRole = "Member";

    private readonly IMembershipRepository _memberships;

    public MembershipService(IMembershipRepository memberships)
    {
        _memberships = memberships;
    }

    public async Task ProvisionAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (await _memberships.ExistsAsync(userId, organizationId, cancellationToken))
        {
            return;
        }

        var membership = new OrganizationMembership(
            userId: userId,
            organizationId: organizationId,
            role: string.IsNullOrWhiteSpace(role) ? DefaultRole : role);

        await _memberships.AddAsync(membership, cancellationToken);
    }

    public Task<OrganizationMembership?> GetPrimaryByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => _memberships.GetPrimaryByUserAsync(userId, cancellationToken);

    public Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
        => _memberships.ListByOrganizationAsync(organizationId, cancellationToken);
}
