using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KWinHub.Hub.Application.Common;
using KWinHub.Hub.Application.Workers;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KWinHub.Hub.Api.Controllers;

public class MediaCallbackOptions
{
    public string SigningSecret { get; set; } = string.Empty;
    public string SignatureHeader { get; set; } = "X-Callback-Signature";
}

public record MediaCallbackPayload(
    Guid AssetId,
    string Status,
    string? ProviderId = null,
    string? PlaybackUrl = null,
    string? Error = null,
    string? ProviderJobId = null);

[ApiController]
[AllowAnonymous]
[Route("api/v1/media-callbacks")]
public class MediaCallbacksController : ControllerBase
{
    private readonly IAssetRepository _assetRepository;
    private readonly IWorkerJobRepository _workerJobRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly MediaCallbackOptions _options;
    private readonly ILogger<MediaCallbacksController> _logger;

    public MediaCallbacksController(
        IAssetRepository assetRepository,
        IWorkerJobRepository workerJobRepository,
        ITransactionManager transactionManager,
        IOptions<MediaCallbackOptions> options,
        ILogger<MediaCallbacksController> logger)
    {
        _assetRepository = assetRepository;
        _workerJobRepository = workerJobRepository;
        _transactionManager = transactionManager;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> HandleCallback(
        [FromBody] MediaCallbackPayload payload,
        CancellationToken cancellationToken)
    {
        if (!ValidateSignature())
        {
            _logger.LogWarning("Media callback rejected: invalid signature.");
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid signature",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        if (payload.AssetId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid payload",
                Detail = "AssetId is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var job = await _workerJobRepository.GetByAssetAndTypeAsync(
            payload.AssetId, WorkerJobType.TranscodeVideo, cancellationToken);

        if (job is null)
        {
            _logger.LogInformation("Media callback for asset {AssetId} ignored: no matching job.", payload.AssetId);
            return NoContent();
        }

        if (job.Status == WorkerJobStatus.Completed || job.Status == WorkerJobStatus.Skipped)
        {
            _logger.LogInformation(
                "Media callback for asset {AssetId} ignored: job already in terminal state {Status}.",
                payload.AssetId, job.Status);
            return NoContent();
        }

        var asset = await _assetRepository.GetByIdAsync(job.OrganizationId, payload.AssetId, cancellationToken);
        if (asset is null)
        {
            _logger.LogWarning("Media callback for asset {AssetId} failed: asset not found.", payload.AssetId);
            return NoContent();
        }

        await using var tx = await _transactionManager.BeginTransactionAsync(cancellationToken);
        try
        {
            switch (payload.Status?.ToLowerInvariant())
            {
                case "completed":
                case "success":
                    if (asset.Status != DataAssetStatus.Ready)
                    {
                        if (asset.Status == DataAssetStatus.Processing)
                        {
                            asset.MarkReady(payload.ProviderId ?? asset.MediaProviderId ?? "provider", payload.PlaybackUrl);
                        }
                        else if (asset.Status is DataAssetStatus.UploadedVerified or DataAssetStatus.ProcessingFailed)
                        {
                            asset.MarkProcessing();
                            asset.MarkReady(payload.ProviderId ?? "provider", payload.PlaybackUrl);
                        }
                    }
                    job.MarkCompleted(JsonSerializer.Serialize(payload));
                    break;

                case "failed":
                case "error":
                    if (asset.Status != DataAssetStatus.Ready)
                    {
                        asset.MarkProcessingFailed(payload.Error ?? "Provider reported failure");
                    }
                    job.MarkFailed(payload.Error ?? "Provider reported failure");
                    break;

                case "processing":
                    job.RenewLease(TimeSpan.FromMinutes(5));
                    break;

                default:
                    _logger.LogWarning(
                        "Media callback for asset {AssetId} has unknown status {Status}.",
                        payload.AssetId, payload.Status);
                    break;
            }

            await _assetRepository.UpdateAsync(asset, cancellationToken);
            await _workerJobRepository.UpdateAsync(job, cancellationToken);
            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Media callback for asset {AssetId} processed. Job status: {JobStatus}. Asset status: {AssetStatus}.",
                payload.AssetId, job.Status, asset.Status);

            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private bool ValidateSignature()
    {
        if (string.IsNullOrWhiteSpace(_options.SigningSecret))
        {
            return true;
        }

        if (!Request.Headers.TryGetValue(_options.SignatureHeader, out var signatureHeader) ||
            string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = reader.ReadToEnd();
        Request.Body.Position = 0;

        var expected = ComputeSignature(body, _options.SigningSecret);
        return string.Equals(signatureHeader.ToString(), expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSignature(string body, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}