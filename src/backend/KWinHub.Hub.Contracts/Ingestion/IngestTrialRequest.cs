namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestTrialRequest(
    int TrialIndex,
    string? ExternalId = null,
    string? Notes = null,
    IReadOnlyList<IngestMetricRequest>? Metrics = null);