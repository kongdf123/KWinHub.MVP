using KWinHub.Hub.Protocols;

namespace KWinHub.Hub.Application.Protocols;

public interface IMetricDefinitionRepository
{
    Task<MetricDefinition?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<MetricDefinition?> GetByCodeAndVersionAsync(Guid organizationId, string code, string version, CancellationToken cancellationToken = default);

    Task<MetricDefinition?> GetByCodeAsync(Guid organizationId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MetricDefinition>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(MetricDefinition definition, CancellationToken cancellationToken = default);
    Task UpdateAsync(MetricDefinition definition, CancellationToken cancellationToken = default);
}