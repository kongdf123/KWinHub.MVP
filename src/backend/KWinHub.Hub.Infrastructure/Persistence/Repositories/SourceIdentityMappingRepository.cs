using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Subjects;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class SourceIdentityMappingRepository : ISourceIdentityMappingRepository
{
    private readonly HubDbContext _dbContext;

    public SourceIdentityMappingRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SourceIdentityMapping?> GetBySourceIdentityAsync(
        Guid organizationId,
        string sourceApp,
        string installationScope,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SourceIdentityMappings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                m => m.OrganizationId == organizationId
                    && m.SourceApp == sourceApp
                    && m.InstallationScope == installationScope
                    && m.ExternalId == externalId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SourceIdentityMapping>> ListByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        string? sourceApp = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SourceIdentityMappings
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(sourceApp))
        {
            query = query.Where(m => m.SourceApp == sourceApp);
        }

        return await query
            .OrderBy(m => m.SourceApp)
            .ThenBy(m => m.ExternalId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SourceIdentityMapping>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SourceIdentityMappings
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.SubjectId == subjectId)
            .OrderBy(m => m.SourceApp)
            .ThenBy(m => m.ExternalId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SourceIdentityMapping mapping, CancellationToken cancellationToken = default)
    {
        await _dbContext.SourceIdentityMappings.AddAsync(mapping, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SourceIdentityMapping mapping, CancellationToken cancellationToken = default)
    {
        _dbContext.SourceIdentityMappings.Update(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}