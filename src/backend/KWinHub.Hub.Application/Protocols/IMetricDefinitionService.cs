using KWinHub.Hub.Contracts.Protocols;

namespace KWinHub.Hub.Application.Protocols;

public interface IMetricDefinitionService
{
    Task<MetricDefinitionResponse> CreateAsync(Guid organizationId, CreateMetricDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<MetricDefinitionResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MetricDefinitionResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<MetricDefinitionResponse?> UpdateAsync(Guid organizationId, Guid id, UpdateMetricDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<MetricDefinitionResponse?> DeprecateAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
}