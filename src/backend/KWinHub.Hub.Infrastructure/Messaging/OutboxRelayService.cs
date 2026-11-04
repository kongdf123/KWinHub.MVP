using KWinHub.Hub.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KWinHub.Hub.Infrastructure.Messaging;

public class OutboxRelayService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxRelayService> _logger;

    private const int BatchSize = 50;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 5;

    public OutboxRelayService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<OutboxRelayService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox relay service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox relay loop failed. Will retry after delay.");
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Outbox relay service stopped.");
    }

    internal async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await outbox.ResetExpiredLeasesAsync(cancellationToken);
        await outbox.ResetFailedForRetryAsync(MaxRetryCount, cancellationToken);

        var messages = await outbox.GetPendingAsync(BatchSize, LeaseDuration, cancellationToken);
        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Relay processing {Count} pending outbox messages.", messages.Count);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                PayloadContractLinter.Validate(message.PayloadJson);

                var envelope = new OutboxEventEnvelope
                {
                    EventId = message.EventId,
                    EventType = message.EventType,
                    OrganizationId = message.OrganizationId,
                    AggregateType = message.AggregateType,
                    AggregateId = message.AggregateId,
                    EventVersion = message.EventVersion,
                    PayloadJson = message.PayloadJson
                };

                await messageBus.PublishAsync(envelope, cancellationToken);
                await outbox.MarkSentAsync(message.Id, cancellationToken);

                _logger.LogInformation(
                    "Published outbox message {MessageId} (event {EventType}).",
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
                await outbox.MarkFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}