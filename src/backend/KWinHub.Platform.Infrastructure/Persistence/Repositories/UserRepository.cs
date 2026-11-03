using KWinHub.Application.Registration;
using KWinHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly KWinHubDbContext _dbContext;

    public UserRepository(KWinHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<(Guid OrganizationId, string OrganizationName, string Role)?> GetPrimaryOrganizationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var organization = await (
                from membership in _dbContext.OrganizationMemberships
                join org in _dbContext.Organizations on membership.OrganizationId equals org.Id
                where membership.UserId == userId && membership.Status == "Active"
                orderby membership.CreatedAt
                select new { org.Id, org.Name, membership.Role })
            .FirstOrDefaultAsync(cancellationToken);

        return organization is null
            ? null
            : (organization.Id, organization.Name, organization.Role);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
