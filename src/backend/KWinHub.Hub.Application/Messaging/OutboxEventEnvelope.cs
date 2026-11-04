namespace KWinHub.Hub.Application.Messaging;

public class OutboxEventEnvelope
{
    public Guid EventId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public Guid OrganizationId { get; init; }

    public string? AggregateType { get; init; }

    public Guid? AggregateId { get; init; }

    public long EventVersion { get; init; }

    public string PayloadJson { get; init; } = string.Empty;
}