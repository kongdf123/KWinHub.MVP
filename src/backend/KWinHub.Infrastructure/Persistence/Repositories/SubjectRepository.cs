using KWinHub.Application.Subjects;
using KWinHub.Domain.Subjects;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly KWinHubDbContext _dbContext;

    public SubjectRepository(KWinHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Subject>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .Where(s => s.OrganizationId == organizationId)
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