namespace KWinHub.Hub.Application.Media;

public record MediaProcessingRequest(
    Guid AssetId,
    Guid OrganizationId,
    string StorageKey,
    string Bucket,
    string MimeType,
    string? ExternalId);

public record MediaProcessingResult(
    bool Success,
    string ProviderId,
    string? PlaybackUrl = null,
    string? Error = null,
    bool Skipped = false,
    string? SkipReason = null);

public interface IMediaProcessor
{
    string ProviderName { get; }

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingRequest request,
        CancellationToken cancellationToken = default);
}