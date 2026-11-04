using KWinHub.Hub.Contracts.Ingestion;

namespace KWinHub.Hub.Application.Ingestion;

public interface IIngestionService
{
    Task<IngestSessionResponse> IngestSessionAsync(
        Guid organizationId,
        IngestSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<IngestionOperationStatusResponse?> GetOperationStatusAsync(
        Guid organizationId,
        string clientOperationId,
        CancellationToken cancellationToken = default);

    Task<IngestionOperationStatusResponse?> GetSessionStatusAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}