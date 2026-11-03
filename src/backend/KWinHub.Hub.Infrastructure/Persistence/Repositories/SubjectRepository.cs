using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Subjects;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly HubDbContext _dbContext;

    public SubjectRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Subject?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == id && s.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Subject>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? subjectType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Subjects
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(normalized) ||
                s.LastName.ToLower().Contains(normalized) ||
                (s.DisplayName != null && s.DisplayName.ToLower().Contains(normalized)) ||
                (s.ExternalId != null && s.ExternalId.ToLower().Contains(normalized)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(subjectType) &&
            Enum.TryParse<SubjectType>(subjectType, true, out var type))
        {
            query = query.Where(s => s.SubjectType == type);
        }

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        await _dbContext.Subjects.AddAsync(subject, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        _dbContext.Subjects.Update(subject);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}