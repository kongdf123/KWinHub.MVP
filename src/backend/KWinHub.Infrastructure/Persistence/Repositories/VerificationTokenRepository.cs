using KWinHub.Application.Registration;
using KWinHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class VerificationTokenRepository : IVerificationTokenRepository
{
    private readonly KWinHubDbContext _dbContext;

    public VerificationTokenRepository(KWinHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailVerificationTokens.AddAsync(token, cancellationToken);
    }

    public async Task<EmailVerificationToken?> FindUsableByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbContext.EmailVerificationTokens
            .SingleOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.ConsumedAt == null && t.ExpiresAt > now,
                cancellationToken);
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Consume();
        }
    }
}
