using KWinHub.Platform.Application.Organizations;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.Platform.Organizations;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Platform.Infrastructure.Persistence.Repositories;

public class OrganizationEventOutbox : IOrganizationEventOutbox
{
    private readonly PlatformDbContext _dbContext;

    public OrganizationEventOutbox(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OrganizationLifecycleEvent evt, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrganizationLifecycleEvents.AddAsync(evt, cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationLifecycleEvent>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OrganizationLifecycleEvents
            .Where(e => e.Status == OrganizationLifecycleEvent.StatusPending)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkSentAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var evt = await _dbContext.OrganizationLifecycleEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (evt is not null)
        {
            evt.MarkSent();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default)
    {
        var evt = await _dbContext.OrganizationLifecycleEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (evt is not null)
        {
            evt.MarkFailed(error);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResetFailedForRetryAsync(int maxRetryCount, CancellationToken cancellationToken = default)
    {
        var failed = await _dbContext.OrganizationLifecycleEvents
            .Where(e => e.Status == OrganizationLifecycleEvent.StatusFailed && e.RetryCount < maxRetryCount)
            .ToListAsync(cancellationToken);

        foreach (var evt in failed)
        {
            evt.ResetForRetry();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<long> GetLatestVersionAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OrganizationLifecycleEvents
            .Where(e => e.OrganizationId == organizationId)
            .MaxAsync(e => (long?)e.EventVersion, cancellationToken) ?? 0;
    }
}