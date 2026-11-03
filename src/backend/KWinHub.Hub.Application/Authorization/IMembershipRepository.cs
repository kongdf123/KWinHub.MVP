using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Authorization;

/// <summary>
/// Queries and persists organization membership to enforce tenant boundaries
/// and to support the internal membership provisioning/query API.
/// </summary>
public interface IMembershipRepository
{
    Task<bool> IsMemberAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);

    Task<OrganizationMembership?> GetAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(OrganizationMembership membership, CancellationToken cancellationToken = default);

    Task UpdateAsync(OrganizationMembership membership, CancellationToken cancellationToken = default);

    Task<OrganizationMembership?> GetPrimaryByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationMembership>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
}

public interface IOrganizationStatusRepository
{
    Task<OrganizationStatus?> GetAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task UpsertAsync(OrganizationStatus status, CancellationToken cancellationToken = default);

    Task<bool> IsOrganizationActiveAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

public interface IOrganizationEventConsumptionRepository
{
    Task<bool> IsConsumedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task MarkConsumedAsync(OrganizationEventConsumption consumption, CancellationToken cancellationToken = default);
}