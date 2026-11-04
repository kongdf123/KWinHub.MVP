using KWinHub.Hub.Messaging;

namespace KWinHub.Hub.Application.Messaging;

public interface IOutbox
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task MarkSentAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);

    Task ResetFailedForRetryAsync(int maxRetryCount, CancellationToken cancellationToken = default);

    Task ResetExpiredLeasesAsync(CancellationToken cancellationToken = default);
}