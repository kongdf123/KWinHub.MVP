using KWinHub.Platform.Audit;

namespace KWinHub.Platform.Application.Audit;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditService(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task RecordAsync(
        string action,
        string entityType,
        string? entityId,
        string summary,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(
            action,
            entityType,
            entityId,
            summary,
            actorUserId,
            actorEmail,
            metadataJson: null,
            ipAddress: ipAddress);

        await _auditLogs.AddAsync(log, cancellationToken);
        await _auditLogs.SaveChangesAsync(cancellationToken);
    }
}