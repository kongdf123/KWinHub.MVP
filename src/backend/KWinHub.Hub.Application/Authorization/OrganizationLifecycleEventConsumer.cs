using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Authorization;

public interface IOrganizationLifecycleEventConsumer
{
    Task ConsumeAsync(
        Guid eventId,
        Guid organizationId,
        string eventType,
        long eventVersion,
        string status,
        CancellationToken cancellationToken = default);
}

public class OrganizationLifecycleEventConsumer : IOrganizationLifecycleEventConsumer
{
    private readonly IOrganizationStatusRepository _statusRepository;
    private readonly IOrganizationEventConsumptionRepository _consumptionRepository;

    public OrganizationLifecycleEventConsumer(
        IOrganizationStatusRepository statusRepository,
        IOrganizationEventConsumptionRepository consumptionRepository)
    {
        _statusRepository = statusRepository;
        _consumptionRepository = consumptionRepository;
    }

    public async Task ConsumeAsync(
        Guid eventId,
        Guid organizationId,
        string eventType,
        long eventVersion,
        string status,
        CancellationToken cancellationToken = default)
    {
        if (await _consumptionRepository.IsConsumedAsync(eventId, cancellationToken))
        {
            return;
        }

        var current = await _statusRepository.GetAsync(organizationId, cancellationToken);
        if (current is not null && current.Version >= eventVersion)
        {
            await _consumptionRepository.MarkConsumedAsync(
                new OrganizationEventConsumption(eventId, organizationId, eventVersion, eventType),
                cancellationToken);
            return;
        }

        var nextStatus = new OrganizationStatus(organizationId, status, eventVersion);
        await _statusRepository.UpsertAsync(nextStatus, cancellationToken);

        await _consumptionRepository.MarkConsumedAsync(
            new OrganizationEventConsumption(eventId, organizationId, eventVersion, eventType),
            cancellationToken);
    }
}