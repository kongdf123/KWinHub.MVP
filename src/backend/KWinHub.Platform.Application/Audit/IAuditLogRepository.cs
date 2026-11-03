using KWinHub.Platform.Audit;

namespace KWinHub.Platform.Application.Audit;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}