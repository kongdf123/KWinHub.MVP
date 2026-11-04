using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface IDataSourceRepository
{
    Task<DataSource?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<DataSource?> GetByCodeAsync(Guid organizationId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataSource>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(DataSource source, CancellationToken cancellationToken = default);
    Task UpdateAsync(DataSource source, CancellationToken cancellationToken = default);
}