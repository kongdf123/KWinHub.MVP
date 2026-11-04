using KWinHub.Hub.Contracts.Results;
using KWinHub.Hub.Results;

namespace KWinHub.Hub.Application.Results;

public class ResultService : IResultService
{
    private readonly IResultRepository _repository;

    public ResultService(IResultRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return result is null ? null : ToResponse(result);
    }

    public async Task<ResultResponse?> GetBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetBySessionAsync(organizationId, sessionId, cancellationToken);
        return result is null ? null : ToResponse(result);
    }

    public async Task<IReadOnlyList<ResultResponse>> ListBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var results = await _repository.ListBySessionAsync(organizationId, sessionId, cancellationToken);
        return results.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<ResultResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var results = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return results.Select(ToResponse).ToList();
    }

    private static ResultResponse ToResponse(Result result) =>
        new(
            Id: result.Id,
            OrganizationId: result.OrganizationId,
            SessionId: result.SessionId,
            ProtocolId: result.ProtocolId,
            ProtocolVersion: result.ProtocolVersion,
            AlgorithmVersion: result.AlgorithmVersion,
            Status: result.Status,
            QualityStatus: result.QualityStatus,
            TrialId: result.TrialId,
            Metrics: result.Metrics.Select(ToMetricResponse).ToList(),
            CreatedAt: result.CreatedAt,
            UpdatedAt: result.UpdatedAt);

    private static MetricValueResponse ToMetricResponse(Metric metric) =>
        new(
            Id: metric.Id,
            MetricDefinitionId: metric.MetricDefinitionId,
            MetricCode: metric.MetricCode,
            MetricName: metric.MetricName,
            ValueType: metric.ValueType.ToString(),
            ScalarValue: metric.ScalarValue,
            LeftValue: metric.LeftValue,
            RightValue: metric.RightValue,
            CategoricalValue: metric.CategoricalValue,
            StructuredValue: metric.StructuredValue,
            Unit: metric.Unit,
            Side: metric.Side);
}