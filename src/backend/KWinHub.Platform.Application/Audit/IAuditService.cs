namespace KWinHub.Platform.Application.Audit;

/// <summary>
/// Application service for recording security-relevant audit events.
/// Audit rows must NEVER contain credentials, tokens, or secrets (AC-17).
/// </summary>
public interface IAuditService
{
    Task RecordAsync(
        string action,
        string entityType,
        string? entityId,
        string summary,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}