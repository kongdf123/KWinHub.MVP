using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Sessions;

public class IngestionOperation : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string ClientOperationId { get; private set; } = string.Empty;

    public Guid? SessionId { get; private set; }

    public string? SessionExternalId { get; private set; }

    public IngestionOperationStatus Status { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? ErrorDetail { get; private set; }

    public string? ContentHash { get; private set; }

    public DateTimeOffset CompletedAt { get; private set; }

    private IngestionOperation()
    {
    }

    public IngestionOperation(Guid organizationId, string clientOperationId, string? contentHash = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ClientOperationId = clientOperationId;
        ContentHash = contentHash;
        Status = IngestionOperationStatus.Pending;
    }

    public void MarkAccepted(Guid sessionId, string? sessionExternalId)
    {
        SessionId = sessionId;
        SessionExternalId = sessionExternalId;
        Status = IngestionOperationStatus.Accepted;
        ErrorCode = null;
        ErrorDetail = null;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string errorCode, string errorDetail)
    {
        Status = IngestionOperationStatus.Failed;
        ErrorCode = errorCode;
        ErrorDetail = errorDetail;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}