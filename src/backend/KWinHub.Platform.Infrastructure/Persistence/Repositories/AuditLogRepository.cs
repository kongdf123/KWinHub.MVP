using KWinHub.Platform.Application.Audit;
using KWinHub.Platform.Audit;
using KWinHub.Platform.Infrastructure.Persistence;

namespace KWinHub.Platform.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly PlatformDbContext _dbContext;

    public AuditLogRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}