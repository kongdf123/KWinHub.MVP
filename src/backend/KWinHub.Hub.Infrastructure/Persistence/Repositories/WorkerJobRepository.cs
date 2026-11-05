using KWinHub.Hub.Application.Workers;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Workers;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class WorkerJobRepository : IWorkerJobRepository
{
    private readonly HubDbContext _dbContext;

    public WorkerJobRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WorkerJob job, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkerJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkerJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkerJobs.FindAsync(
            new object[] { jobId }, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkerJob>> GetPendingAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var pending = await _dbContext.WorkerJobs
            .Where(j => j.Status == WorkerJobStatus.Pending)
            .OrderBy(j => j.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var expired = await _dbContext.WorkerJobs
            .Where(j => j.Status == WorkerJobStatus.Processing)
            .AsAsyncEnumerable()
            .Where(j => j.LeaseExpiresAt < now)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var jobs = pending
            .Concat(expired)
            .OrderBy(j => j.Id)
            .Take(batchSize)
            .ToList();

        foreach (var job in jobs)
        {
            job.Claim(leaseDuration);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return jobs;
    }

    public async Task UpdateAsync(WorkerJob job, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkerJobs.Update(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetExpiredLeasesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _dbContext.WorkerJobs
            .Where(j => j.Status == WorkerJobStatus.Processing)
            .AsAsyncEnumerable()
            .Where(j => j.LeaseExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var job in expired)
        {
            job.ResetLease();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkerJob>> GetByAssetIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkerJobs
            .Where(j => j.AssetId == assetId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkerJob>> GetByOrganizationIdAsync(
        Guid organizationId,
        string? status = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WorkerJobs
            .Where(j => j.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(j => j.Status == status);
        }

        return await query
            .AsAsyncEnumerable()
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkerJob>> GetStuckJobsAsync(
        TimeSpan processingStuckThreshold,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(processingStuckThreshold);

        return await _dbContext.WorkerJobs
            .Where(j => j.Status == WorkerJobStatus.Processing)
            .AsAsyncEnumerable()
            .Where(j => j.UpdatedAt < cutoff)
            .OrderBy(j => j.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task ResetJobForRetryAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.WorkerJobs.FindAsync(
            new object[] { jobId }, cancellationToken);

        if (job is null)
        {
            return;
        }

        if (job.Status != WorkerJobStatus.Failed && job.Status != WorkerJobStatus.Skipped)
        {
            return;
        }

        job.ResetForRetry();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ResetFailedForRetryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var failed = await _dbContext.WorkerJobs
            .Where(j => j.OrganizationId == organizationId && j.Status == WorkerJobStatus.Failed)
            .ToListAsync(cancellationToken);

        foreach (var job in failed)
        {
            job.ResetForRetry();
        }

        if (failed.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return failed.Count;
    }
}