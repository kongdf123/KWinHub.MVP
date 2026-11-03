namespace KWinHub.Platform.Application.CrossContext;

/// <summary>
/// A user's membership in an organization as owned by the Hub context.
/// Carries only primitives so the Platform context never references Hub
/// domain entities or its database schema (ADR-0001 bounded-context boundary).
/// </summary>
public sealed record HubMembership(
    Guid UserId,
    string Role,
    string Status,
    DateTimeOffset JoinedAt);

/// <summary>The user's earliest active membership, used to resolve the active tenant at login.</summary>
public sealed record HubPrimaryMembership(Guid OrganizationId, string Role);

/// <summary>Workspace usage counts owned by the Hub context for a single organization.</summary>
public sealed record HubOrganizationStats(
    Guid OrganizationId,
    int MemberCount,
    int SubjectCount,
    int SessionCount,
    int ResultCount);

/// <summary>
/// Port used by the Platform context to reach membership and workspace data that
/// is owned by the Hub context. In M0 this is satisfied over an internal,
/// API-key-protected HTTP API; M1-2 replaces the provisioning write path with an
/// outbox + event relay while the read calls remain internal queries.
/// </summary>
public interface IHubGateway
{
    Task ProvisionAdminMembershipAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<HubPrimaryMembership?> GetPrimaryMembershipAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubMembership>> ListMembershipsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubOrganizationStats>> GetOrganizationStatsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetUserMembershipCountsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task PublishOrganizationLifecycleEventAsync(
        Guid eventId,
        Guid organizationId,
        string eventType,
        long eventVersion,
        string payloadJson,
        CancellationToken cancellationToken = default);
}