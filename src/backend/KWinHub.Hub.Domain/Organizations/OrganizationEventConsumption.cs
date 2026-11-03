using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Organizations;

public class OrganizationEventConsumption : BaseEntity<Guid>
{
    public Guid EventId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public long EventVersion { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public DateTimeOffset ConsumedAt { get; private set; }

    private OrganizationEventConsumption()
    {
    }

    public OrganizationEventConsumption(Guid eventId, Guid organizationId, long eventVersion, string eventType)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        OrganizationId = organizationId;
        EventVersion = eventVersion;
        EventType = eventType;
        ConsumedAt = DateTimeOffset.UtcNow;
    }
}