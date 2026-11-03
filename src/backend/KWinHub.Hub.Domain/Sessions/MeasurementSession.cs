using KWinHub.Domain.Common;

namespace KWinHub.Domain.Sessions;

public class MeasurementSession : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid SubjectId { get; private set; }

    public Guid ProtocolId { get; private set; }

    public string ProtocolVersion { get; private set; } = string.Empty;

    public Guid? SourceId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public SessionStatus Status { get; private set; } = SessionStatus.Received;

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? Timezone { get; private set; }

    public string? ClientApplication { get; private set; }

    public string? ClientVersion { get; private set; }

    public string? ExternalId { get; private set; }

    private MeasurementSession()
    {
    }

    public MeasurementSession(
        Guid organizationId,
        Guid subjectId,
        Guid protocolId,
        string protocolVersion,
        Guid? sourceId = null,
        Guid? deviceId = null,
        string? clientApplication = null,
        string? clientVersion = null,
        string? externalId = null,
        string? timezone = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SubjectId = subjectId;
        ProtocolId = protocolId;
        ProtocolVersion = protocolVersion;
        SourceId = sourceId;
        DeviceId = deviceId;
        ClientApplication = clientApplication;
        ClientVersion = clientVersion;
        ExternalId = externalId;
        Timezone = timezone;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void TransitionTo(SessionStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == SessionStatus.Published)
        {
            CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}