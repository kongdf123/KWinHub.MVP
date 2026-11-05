namespace KWinHub.Hub.Reports;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    Task<Report?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Report>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Report>> ListBySubjectAsync(Guid organizationId, Guid subjectId, CancellationToken cancellationToken = default);
}