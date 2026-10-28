namespace KWinHub.Application.Authorization;

/// <summary>
/// Queries organization membership to enforce tenant boundaries.
/// </summary>
public interface IMembershipRepository
{
    Task<bool> IsMemberAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
}
