namespace KWinHub.Contracts.Sessions;

public record SessionResponse(
    Guid Id,
    Guid OrganizationId,
    Guid SubjectId,
    Guid ProtocolId,
    string ProtocolVersion,
    Guid? SourceId,
    Guid? DeviceId,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ClientApplication,
    string? ClientVersion,
    string? ExternalId,
    string? Timezone,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
