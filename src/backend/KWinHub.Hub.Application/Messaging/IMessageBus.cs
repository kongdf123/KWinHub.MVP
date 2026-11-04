namespace KWinHub.Hub.Application.Messaging;

public interface IMessageBus
{
    Task PublishAsync(OutboxEventEnvelope envelope, CancellationToken cancellationToken = default);
}