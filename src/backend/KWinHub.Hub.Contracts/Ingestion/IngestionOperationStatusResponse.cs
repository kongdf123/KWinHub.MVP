namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestionOperationStatusResponse(
    Guid OperationId,
    string ClientOperationId,
    string Status,
    Guid? SessionId = null,
    string? SessionExternalId = null,
    string? ErrorCode = null,
    string? ErrorDetail = null,
    DateTimeOffset CompletedAt = default);