namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestTrialResult(
    int TrialIndex,
    Guid TrialId,
    string Status);

public record IngestSessionResponse(
    Guid SessionId,
    string? SessionExternalId,
    string Status,
    IReadOnlyList<IngestTrialResult>? Trials = null,
    IReadOnlyList<IngestAssetPlaceholderResult>? AssetPlaceholders = null,
    IReadOnlyList<IngestionError>? Errors = null);