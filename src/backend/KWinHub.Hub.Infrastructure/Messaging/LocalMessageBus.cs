using System.Collections.Concurrent;
using KWinHub.Hub.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace KWinHub.Hub.Infrastructure.Messaging;

public class LocalMessageBus : IMessageBus
{
    private readonly ConcurrentQueue<OutboxEventEnvelope> _queue = new();
    private readonly ILogger<LocalMessageBus> _logger;

    public LocalMessageBus(ILogger<LocalMessageBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(OutboxEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(envelope);
        _logger.LogInformation(
            "Published event {EventType} for org {OrganizationId} (eventId={EventId}) to local queue",
            envelope.EventType, envelope.OrganizationId, envelope.EventId);
        return Task.CompletedTask;
    }

    public IReadOnlyList<OutboxEventEnvelope> DrainAll()
    {
        var list = new List<OutboxEventEnvelope>();
        while (_queue.TryDequeue(out var message))
        {
            list.Add(message);
        }

        return list;
    }

    public bool TryDequeue(out OutboxEventEnvelope? envelope)
    {
        return _queue.TryDequeue(out envelope);
    }
}