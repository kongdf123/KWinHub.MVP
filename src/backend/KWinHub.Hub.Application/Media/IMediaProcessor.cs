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
    string? SkipReason = null,
    string? ProviderJobId = null);

public enum MediaProviderState
{
    Unknown,
    Processing,
    Completed,
    Failed
}

public record MediaStatusQueryResult(
    MediaProviderState State,
    string? ProviderId = null,
    string? PlaybackUrl = null,
    string? ErrorMessage = null);

public interface IMediaProcessor
{
    string ProviderName { get; }

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaStatusQueryResult> QueryStatusAsync(
        string providerJobId,
        CancellationToken cancellationToken = default);
}