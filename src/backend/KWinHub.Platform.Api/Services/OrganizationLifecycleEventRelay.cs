using KWinHub.Platform.Application.CrossContext;
using KWinHub.Platform.Application.Organizations;
using KWinHub.Platform.Organizations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KWinHub.Platform.Api.Services;

public class OrganizationLifecycleEventRelay : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrganizationLifecycleEventRelay> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly int _batchSize;
    private readonly int _maxRetryCount;

    public OrganizationLifecycleEventRelay(
        IServiceScopeFactory scopeFactory,
        ILogger<OrganizationLifecycleEventRelay> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("LifecycleEvents:PollIntervalSeconds", 5));
        _batchSize = configuration.GetValue<int>("LifecycleEvents:BatchSize", 10);
        _maxRetryCount = configuration.GetValue<int>("LifecycleEvents:MaxRetryCount", 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RelayBatchAsync(stoppingToken);
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lifecycle event relay loop failed; will retry after interval.");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }

    private async Task RelayBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOrganizationEventOutbox>();
        var hubGateway = scope.ServiceProvider.GetRequiredService<IHubGateway>();

        await outbox.ResetFailedForRetryAsync(_maxRetryCount, cancellationToken);

        var events = await outbox.GetPendingAsync(_batchSize, cancellationToken);
        foreach (var evt in events)
        {
            try
            {
                await hubGateway.PublishOrganizationLifecycleEventAsync(
                    evt.Id,
                    evt.OrganizationId,
                    evt.EventType,
                    evt.EventVersion,
                    evt.PayloadJson,
                    cancellationToken);

                await outbox.MarkSentAsync(evt.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to relay lifecycle event {EventId} for org {OrganizationId}; marking as failed.",
                    evt.Id,
                    evt.OrganizationId);
                await outbox.MarkFailedAsync(evt.Id, ex.Message, cancellationToken);
            }
        }
    }
}