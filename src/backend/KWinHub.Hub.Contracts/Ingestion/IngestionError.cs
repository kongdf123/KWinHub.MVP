namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestionError(
    string Code,
    string Message,
    string? Field = null);