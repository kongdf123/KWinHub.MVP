using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Sessions;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly HubDbContext _dbContext;

    public SessionRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MeasurementSession?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MeasurementSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == id && s.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<MeasurementSession>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.MeasurementSessions
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        return sessions.OrderByDescending(s => s.StartedAt).ToList();
    }

    public async Task<IReadOnlyList<MeasurementSession>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.MeasurementSessions
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        return sessions.OrderByDescending(s => s.StartedAt).ToList();
    }

    public async Task<IReadOnlyList<MeasurementSession>> ListBySubjectFilteredAsync(
        Guid organizationId,
        Guid subjectId,
        Guid? protocolId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MeasurementSessions
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.SubjectId == subjectId);

        if (protocolId.HasValue)
        {
            query = query.Where(s => s.ProtocolId == protocolId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(s => s.StartedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.StartedAt <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<SessionStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(s => s.Status == parsedStatus);
            }
        }

        var sessions = await query.ToListAsync(cancellationToken);
        return sessions.OrderByDescending(s => s.StartedAt).ToList();
    }

    public async Task AddAsync(MeasurementSession session, CancellationToken cancellationToken = default)
    {
        await _dbContext.MeasurementSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MeasurementSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.MeasurementSessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MeasurementSession?> GetByExternalIdAsync(
        Guid organizationId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MeasurementSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.ExternalId == externalId,
                cancellationToken);
    }
}