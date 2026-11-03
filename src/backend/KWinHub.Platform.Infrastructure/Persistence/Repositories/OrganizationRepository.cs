using KWinHub.Application.Registration;
using KWinHub.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly KWinHubDbContext _dbContext;

    public OrganizationRepository(KWinHubDbContext dbContext)
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

    public async Task AddMembershipAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.OrganizationMemberships.AddAsync(membership, cancellationToken);
    }
}
