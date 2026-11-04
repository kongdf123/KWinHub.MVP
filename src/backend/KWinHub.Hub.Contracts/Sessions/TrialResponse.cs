namespace KWinHub.Hub.Contracts.Sessions;

public record TrialResponse(
    Guid Id,
    Guid OrganizationId,
    Guid SessionId,
    int TrialIndex,
    string Status,
    string? ExternalId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);