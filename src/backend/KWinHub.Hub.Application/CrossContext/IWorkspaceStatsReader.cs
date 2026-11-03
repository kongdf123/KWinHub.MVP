namespace KWinHub.Hub.Application.CrossContext;

/// <summary>Hub-owned usage counters for one organization, exposed to Platform via the internal API.</summary>
public sealed record OrganizationWorkspaceStats(
    Guid OrganizationId,
    int MemberCount,
    int SubjectCount,
    int SessionCount,
    int ResultCount);

/// <summary>
/// Aggregates Hub-owned workspace counters for the Platform dashboard. Reads only
/// Hub schema tables; implemented in Hub.Infrastructure against HubDbContext.
/// </summary>
public interface IWorkspaceStatsReader
{
    Task<IReadOnlyList<OrganizationWorkspaceStats>> GetOrganizationStatsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountMembershipsByUserAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
}
