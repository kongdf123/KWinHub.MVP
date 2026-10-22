namespace KWinHub.Contracts.Platform;

public record AuditLogResponse(
    Guid Id,
    Guid? ActorUserId,
    string? ActorEmail,
    string Action,
    string EntityType,
    string? EntityId,
    string Summary,
    string? MetadataJson,
    DateTimeOffset CreatedAt);

public record PlatformDashboardResponse(
    int TotalOrganizations,
    int ActiveOrganizations,
    int SuspendedOrganizations,
    int IndividualOrganizations,
    int TotalUsers,
    int ActiveUsers,
    int PendingVerificationUsers,
    int TotalProtocolDefinitions,
    int TotalMetrics,
    IReadOnlyList<PlatformOrganizationResponse> RecentOrganizations,
    IReadOnlyList<AuditLogResponse> RecentAuditLogs);
