namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestAssetPlaceholderRequest(
    string AssetType,
    string ExternalId,
    long Size,
    string? MimeType = null,
    string? Checksum = null,
    int? TrialIndex = null);