using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.Platform.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Platform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PlatformDbContext _dbContext;

    public UserRepository(PlatformDbContext dbContext)
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

    public async Task<string?> GetSecurityStampAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => u.SecurityStamp)
            .SingleOrDefaultAsync(cancellationToken);
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}