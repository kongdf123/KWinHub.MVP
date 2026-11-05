using System.Text.Json;
using KWinHub.Hub.Application.Common;
using KWinHub.Hub.Application.Media;
using KWinHub.Hub.Application.Workers;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KWinHub.Hub.Infrastructure.Workers;

public class MediaWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MediaWorkerService> _logger;

    private const int BatchSize = 10;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan StuckJobThreshold = TimeSpan.FromMinutes(10);

    public MediaWorkerService(
        IServiceScopeFactory scopeFactory,
        ILogger<MediaWorkerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Media worker service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Media worker loop failed. Will retry after delay.");
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Media worker service stopped.");
    }

    internal async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IWorkerJobRepository>();
        var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var mediaProcessor = scope.ServiceProvider.GetRequiredService<IMediaProcessor>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();

        await jobRepository.ResetExpiredLeasesAsync(cancellationToken);
        await ReconcileStuckJobsAsync(jobRepository, cancellationToken);

        var jobs = await jobRepository.GetPendingAsync(BatchSize, LeaseDuration, cancellationToken);
        if (jobs.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Media worker processing {Count} pending jobs.", jobs.Count);

        foreach (var job in jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessJobAsync(job, jobRepository, assetRepository, mediaProcessor, transactionManager, cancellationToken);
        }
    }

    private async Task ReconcileStuckJobsAsync(
        IWorkerJobRepository jobRepository,
        CancellationToken cancellationToken)
    {
        var stuckJobs = await jobRepository.GetStuckJobsAsync(StuckJobThreshold, cancellationToken);
        if (stuckJobs.Count == 0)
        {
            return;
        }

        _logger.LogWarning(
            "Media worker detected {Count} stuck processing jobs (older than {Threshold}). Resetting to pending.",
            stuckJobs.Count, StuckJobThreshold);

        foreach (var job in stuckJobs)
        {
            job.ResetLease();
            await jobRepository.UpdateAsync(job, cancellationToken);
        }
    }

    private async Task ProcessJobAsync(
        WorkerJob job,
        IWorkerJobRepository jobRepository,
        IAssetRepository assetRepository,
        IMediaProcessor mediaProcessor,
        ITransactionManager transactionManager,
        CancellationToken cancellationToken)
    {
        try
        {
            var asset = await assetRepository.GetByIdAsync(job.OrganizationId, job.AssetId, cancellationToken);
            if (asset is null)
            {
                job.MarkSkipped("Asset not found");
                await jobRepository.UpdateAsync(job, cancellationToken);
                _logger.LogWarning("Media worker job {JobId} skipped: asset {AssetId} not found.", job.Id, job.AssetId);
                return;
            }

            if (asset.Status == DataAssetStatus.Ready)
            {
                job.MarkCompleted();
                await jobRepository.UpdateAsync(job, cancellationToken);
                return;
            }

            if (asset.Status != DataAssetStatus.UploadedVerified &&
                asset.Status != DataAssetStatus.Processing &&
                asset.Status != DataAssetStatus.ProcessingFailed)
            {
                job.MarkSkipped($"Asset is in status {asset.Status}, not eligible for processing.");
                await jobRepository.UpdateAsync(job, cancellationToken);
                return;
            }

            if (asset.Status != DataAssetStatus.Processing)
            {
                asset.MarkProcessing();
                await assetRepository.UpdateAsync(asset, cancellationToken);
            }

            var payload = string.IsNullOrEmpty(job.PayloadJson)
                ? new MediaProcessingRequest(asset.Id, asset.OrganizationId, asset.StorageKey!, asset.StorageBucket!, asset.MimeType ?? "application/octet-stream", asset.ExternalId)
                : JsonSerializer.Deserialize<MediaProcessingRequest>(job.PayloadJson!)!;

            var result = await mediaProcessor.ProcessAsync(payload, cancellationToken);

            if (result.Skipped)
            {
                await using var tx = await transactionManager.BeginTransactionAsync(cancellationToken);
                try
                {
                    asset.MarkReady(result.ProviderId, result.PlaybackUrl);
                    await assetRepository.UpdateAsync(asset, cancellationToken);
                    job.MarkCompleted(JsonSerializer.Serialize(result));
                    await jobRepository.UpdateAsync(job, cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    _logger.LogInformation(
                        "Media worker job {JobId} for asset {AssetId} completed (skipped by provider).",
                        job.Id, job.AssetId);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else if (result.Success)
            {
                await using var tx = await transactionManager.BeginTransactionAsync(cancellationToken);
                try
                {
                    asset.MarkReady(result.ProviderId, result.PlaybackUrl);
                    await assetRepository.UpdateAsync(asset, cancellationToken);
                    job.MarkCompleted(JsonSerializer.Serialize(result));
                    await jobRepository.UpdateAsync(job, cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    _logger.LogInformation(
                        "Media worker job {JobId} for asset {AssetId} completed successfully.",
                        job.Id, job.AssetId);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                await using var tx = await transactionManager.BeginTransactionAsync(cancellationToken);
                try
                {
                    asset.MarkProcessingFailed(result.Error ?? "Unknown processing error");
                    await assetRepository.UpdateAsync(asset, cancellationToken);
                    job.MarkFailed(result.Error ?? "Unknown processing error");
                    await jobRepository.UpdateAsync(job, cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    _logger.LogWarning(
                        "Media worker job {JobId} for asset {AssetId} failed (attempt {Attempt}/{MaxAttempts}): {Error}",
                        job.Id, job.AssetId, job.Attempt, job.MaxAttempts, result.Error);
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing media worker job {JobId}.", job.Id);
            try
            {
                job.MarkFailed(ex.Message);
                await jobRepository.UpdateAsync(job, cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update job {JobId} status after error.", job.Id);
            }
        }
    }
}