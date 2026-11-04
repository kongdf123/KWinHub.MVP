namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestSessionRequest(
    string Version,
    string ClientOperationId,
    Guid SubjectId,
    Guid ProtocolId,
    string ProtocolVersion,
    string? ExternalId = null,
    Guid? SourceId = null,
    Guid? DeviceId = null,
    DateTimeOffset? StartedAt = null,
    string? Timezone = null,
    string? ClientApplication = null,
    string? ClientVersion = null,
    bool IsCorrection = false,
    IReadOnlyList<IngestTrialRequest>? Trials = null,
    IReadOnlyList<IngestAssetPlaceholderRequest>? AssetPlaceholders = null);