namespace KWinHub.Platform.Organizations;

public sealed record OrganizationLifecycleEventPayload(
    Guid OrganizationId,
    string EventType,
    long Version,
    string Status,
    DateTimeOffset OccurredAt);