using KWinHub.Hub.Contracts.Protocols;
using KWinHub.Hub.Protocols;

namespace KWinHub.Hub.Application.Protocols;

public class MetricDefinitionService : IMetricDefinitionService
{
    private readonly IMetricDefinitionRepository _repository;

    public MetricDefinitionService(IMetricDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<MetricDefinitionResponse> CreateAsync(
        Guid organizationId,
        CreateMetricDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<MetricValueType>(request.ValueType, true, out var valueType))
        {
            throw new ArgumentException($"Invalid value type: {request.ValueType}");
        }

        var definition = new MetricDefinition(
            organizationId: organizationId,
            code: request.Code,
            name: request.Name,
            valueType: valueType,
            unit: request.Unit,
            version: request.Version,
            method: request.Method,
            algorithmVersion: request.AlgorithmVersion,
            category: request.Category,
            description: request.Description);

        await _repository.AddAsync(definition, cancellationToken);
        return ToResponse(definition);
    }

    public async Task<MetricDefinitionResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return definition is null ? null : ToResponse(definition);
    }

    public async Task<IReadOnlyList<MetricDefinitionResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var definitions = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return definitions.Select(ToResponse).ToList();
    }

    public async Task<MetricDefinitionResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateMetricDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (definition is null) return null;

        if (!Enum.TryParse<MetricValueType>(request.ValueType, true, out var valueType))
        {
            throw new ArgumentException($"Invalid value type: {request.ValueType}");
        }

        definition.Update(
            name: request.Name,
            valueType: valueType,
            unit: request.Unit,
            version: request.Version,
            method: request.Method,
            algorithmVersion: request.AlgorithmVersion,
            category: request.Category,
            description: request.Description);

        await _repository.UpdateAsync(definition, cancellationToken);
        return ToResponse(definition);
    }

    public async Task<MetricDefinitionResponse?> DeprecateAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (definition is null) return null;

        definition.Deprecate();
        await _repository.UpdateAsync(definition, cancellationToken);
        return ToResponse(definition);
    }

    private static MetricDefinitionResponse ToResponse(MetricDefinition definition) =>
        new(
            Id: definition.Id,
            OrganizationId: definition.OrganizationId,
            Code: definition.Code,
            Name: definition.Name,
            ValueType: definition.ValueType.ToString(),
            Unit: definition.Unit,
            Version: definition.Version,
            Method: definition.Method,
            AlgorithmVersion: definition.AlgorithmVersion,
            Category: definition.Category,
            Description: definition.Description,
            Status: definition.Status,
            CreatedAt: definition.CreatedAt,
            UpdatedAt: definition.UpdatedAt);
}