using KWinHub.Hub.Contracts.History;

namespace KWinHub.Hub.Application.History;

public interface IHistoryService
{
    Task<HistoryListResponse> ListSubjectHistoryAsync(
        Guid organizationId,
        Guid subjectId,
        HistoryFilters? filters = null,
        CancellationToken cancellationToken = default);

    Task<ComparisonResponse> CompareSessionsAsync(
        Guid organizationId,
        Guid subjectId,
        IReadOnlyList<Guid> sessionIds,
        CancellationToken cancellationToken = default);
}