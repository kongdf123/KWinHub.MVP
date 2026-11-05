using KWinHub.Hub.Workers;

namespace KWinHub.Hub.Application.Workers;

public record WorkerJobResponse(
    Guid Id,
    Guid AssetId,
    Guid OrganizationId,
    string JobType,
    string Status,
    int Attempt,
    int MaxAttempts,
    string? PayloadJson,
    string? ResultJson,
    DateTimeOffset? LeaseExpiresAt,
    string? LastError,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public interface IWorkerJobRepository
{
    Task AddAsync(WorkerJob job, CancellationToken cancellationToken = default);

    Task<WorkerJob?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkerJob>> GetPendingAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkerJob job, CancellationToken cancellationToken = default);

    Task ResetExpiredLeasesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkerJob>> GetByAssetIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkerJob>> GetByOrganizationIdAsync(
        Guid organizationId,
        string? status = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkerJob>> GetStuckJobsAsync(
        TimeSpan processingStuckThreshold,
        CancellationToken cancellationToken = default);

    Task ResetJobForRetryAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<int> ResetFailedForRetryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}