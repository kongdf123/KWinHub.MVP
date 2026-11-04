using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Sessions;

public class Trial : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid SessionId { get; private set; }

    public int TrialIndex { get; private set; }

    public TrialStatus Status { get; private set; } = TrialStatus.Pending;

    public string? ExternalId { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? Notes { get; private set; }

    private Trial()
    {
    }

    public Trial(
        Guid organizationId,
        Guid sessionId,
        int trialIndex,
        string? externalId = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SessionId = sessionId;
        TrialIndex = trialIndex;
        ExternalId = externalId;
        Notes = notes;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        Status = TrialStatus.InProgress;
        StartedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        Status = TrialStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = TrialStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = TrialStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}