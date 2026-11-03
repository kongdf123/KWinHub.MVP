using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.Platform.Organizations;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Platform.Infrastructure.Persistence.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly PlatformDbContext _dbContext;

    public OrganizationRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _dbContext.Organizations.AddAsync(organization, cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
