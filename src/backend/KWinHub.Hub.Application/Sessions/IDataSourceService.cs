using KWinHub.Hub.Contracts.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface IDataSourceService
{
    Task<DataSourceResponse> CreateAsync(Guid organizationId, CreateDataSourceRequest request, CancellationToken cancellationToken = default);
    Task<DataSourceResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataSourceResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<DataSourceResponse?> DeactivateAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
}