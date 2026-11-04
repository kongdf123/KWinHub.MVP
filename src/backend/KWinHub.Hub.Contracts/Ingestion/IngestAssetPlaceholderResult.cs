namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestAssetPlaceholderResult(
    string AssetType,
    string ExternalId,
    long Size,
    string Status);