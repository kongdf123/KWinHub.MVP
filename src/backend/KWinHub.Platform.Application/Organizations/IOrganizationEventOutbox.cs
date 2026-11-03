using KWinHub.Platform.Organizations;

namespace KWinHub.Platform.Application.Organizations;

public interface IOrganizationEventOutbox
{
    Task AddAsync(OrganizationLifecycleEvent evt, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationLifecycleEvent>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkSentAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(Guid eventId, string error, CancellationToken cancellationToken = default);

    Task ResetFailedForRetryAsync(int maxRetryCount, CancellationToken cancellationToken = default);

    Task<long> GetLatestVersionAsync(Guid organizationId, CancellationToken cancellationToken = default);
}