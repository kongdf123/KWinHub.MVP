using KWinHub.Hub.Application.Messaging;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Messaging;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutbox
{
    private readonly HubDbContext _dbContext;

    public OutboxRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var pending = await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processing = await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Processing)
            .OrderBy(m => m.Id)
            .AsAsyncEnumerable()
            .Where(m => m.LeaseExpiresAt < now)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var messages = pending
            .Concat(processing)
            .OrderBy(m => m.Id)
            .Take(batchSize)
            .ToList();

        foreach (var message in messages)
        {
            message.Claim(leaseDuration);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return messages;
    }

    public async Task MarkSentAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.OutboxMessages.FindAsync(
            new object[] { messageId }, cancellationToken);
        if (message is not null)
        {
            message.MarkSent();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.OutboxMessages.FindAsync(
            new object[] { messageId }, cancellationToken);
        if (message is not null)
        {
            message.MarkFailed(error);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResetFailedForRetryAsync(int maxRetryCount, CancellationToken cancellationToken = default)
    {
        var failed = await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Failed && m.RetryCount < maxRetryCount)
            .ToListAsync(cancellationToken);

        foreach (var message in failed)
        {
            message.ResetForRetry();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetExpiredLeasesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Processing)
            .AsAsyncEnumerable()
            .Where(m => m.LeaseExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var message in expired)
        {
            message.ResetForRetry();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}