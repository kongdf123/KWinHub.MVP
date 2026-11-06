using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.History;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;

namespace KWinHub.Hub.Application.History;

public class HistoryService : IHistoryService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IProtocolRepository _protocolRepository;

    public HistoryService(
        ISessionRepository sessionRepository,
        IResultRepository resultRepository,
        IProtocolRepository protocolRepository)
    {
        _sessionRepository = sessionRepository;
        _resultRepository = resultRepository;
        _protocolRepository = protocolRepository;
    }

    public async Task<HistoryListResponse> ListSubjectHistoryAsync(
        Guid organizationId,
        Guid subjectId,
        HistoryFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        filters ??= new HistoryFilters();

        var sessions = await _sessionRepository.ListBySubjectFilteredAsync(
            organizationId,
            subjectId,
            filters.ProtocolId,
            filters.From,
            filters.To,
            filters.Status,
            cancellationToken);

        if (sessions.Count == 0)
        {
            return new HistoryListResponse(subjectId, 0, Array.Empty<HistorySessionResponse>());
        }

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var results = await _resultRepository.ListBySessionsAsync(organizationId, sessionIds, cancellationToken);
        var resultsBySession = results.ToLookup(r => r.SessionId);

        var protocolIds = sessions.Select(s => s.ProtocolId).Distinct().ToList();
        var protocols = await _protocolRepository.ListByOrganizationAsync(organizationId, cancellationToken);
        var protocolByProtocolId = protocols.ToDictionary(p => p.Id);

        var responses = sessions.Select(session =>
        {
            var result = resultsBySession[session.Id].FirstOrDefault();
            var protocol = protocolByProtocolId.TryGetValue(session.ProtocolId, out var p) ? p : null;

            return new HistorySessionResponse(
                SessionId: session.Id,
                SubjectId: session.SubjectId,
                ProtocolId: session.ProtocolId,
                ProtocolCode: protocol?.Code ?? string.Empty,
                ProtocolVersion: session.ProtocolVersion,
                Status: session.Status.ToString(),
                StartedAt: session.StartedAt,
                CompletedAt: session.CompletedAt,
                ExternalId: session.ExternalId,
                ResultId: result?.Id,
                ResultStatus: result?.Status,
                QualityStatus: result?.QualityStatus,
                Metrics: result?.Metrics.Select(ToHistoryMetric).ToList()
                    ?? (IReadOnlyList<HistoryMetricResponse>)Array.Empty<HistoryMetricResponse>());
        }).ToList();

        return new HistoryListResponse(subjectId, responses.Count, responses);
    }

    public async Task<ComparisonResponse> CompareSessionsAsync(
        Guid organizationId,
        Guid subjectId,
        IReadOnlyList<Guid> sessionIds,
        CancellationToken cancellationToken = default)
    {
        if (sessionIds.Count == 0)
        {
            return new ComparisonResponse(
                subjectId,
                Array.Empty<Guid>(),
                Array.Empty<CompatibleMetricGroupResponse>(),
                Array.Empty<string>());
        }

        var sessions = await _sessionRepository.ListBySubjectFilteredAsync(
            organizationId,
            subjectId,
            cancellationToken: cancellationToken);

        var requestedSessions = sessions.Where(s => sessionIds.Contains(s.Id)).ToList();
        var validSessionIds = requestedSessions.Select(s => s.Id).ToList();

        var results = await _resultRepository.ListBySessionsAsync(organizationId, validSessionIds, cancellationToken);
        var resultsBySession = results.ToDictionary(r => r.SessionId);

        var sessionBySessionId = requestedSessions.ToDictionary(s => s.Id);

        var allMetricCodes = results
            .SelectMany(r => r.Metrics)
            .Select(m => m.MetricCode)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var compatibleGroups = new List<CompatibleMetricGroupResponse>();
        var excludedCodes = new List<string>();

        foreach (var metricCode in allMetricCodes)
        {
            var group = BuildCompatibleMetricGroup(
                metricCode,
                validSessionIds,
                resultsBySession,
                sessionBySessionId);

            if (group is null)
            {
                excludedCodes.Add(metricCode);
            }
            else
            {
                compatibleGroups.Add(group);
            }
        }

        return new ComparisonResponse(
            subjectId,
            validSessionIds,
            compatibleGroups,
            excludedCodes);
    }

    private static CompatibleMetricGroupResponse? BuildCompatibleMetricGroup(
        string metricCode,
        IReadOnlyList<Guid> sessionIds,
        IReadOnlyDictionary<Guid, Result> resultsBySession,
        IReadOnlyDictionary<Guid, KWinHub.Hub.Sessions.MeasurementSession> sessionBySessionId)
    {
        var points = new List<MetricComparisonPointResponse>();
        var exclusions = new List<MetricExclusionResponse>();

        string? canonicalUnit = null;
        string? canonicalValueType = null;
        string? metricName = null;

        foreach (var sessionId in sessionIds)
        {
            if (!resultsBySession.TryGetValue(sessionId, out var result))
            {
                exclusions.Add(new MetricExclusionResponse(sessionId, "No published result for session"));
                continue;
            }

            var metric = result.Metrics.FirstOrDefault(m => m.MetricCode == metricCode);
            if (metric is null)
            {
                exclusions.Add(new MetricExclusionResponse(sessionId, "Metric not present in result"));
                continue;
            }

            if (canonicalUnit is null)
            {
                canonicalUnit = metric.Unit;
                canonicalValueType = metric.ValueType.ToString();
                metricName = metric.MetricName;
            }
            else if (metric.Unit != canonicalUnit)
            {
                exclusions.Add(new MetricExclusionResponse(
                    sessionId,
                    $"Unit mismatch: expected '{canonicalUnit}', got '{metric.Unit}'"));
                continue;
            }
            else if (metric.ValueType.ToString() != canonicalValueType)
            {
                exclusions.Add(new MetricExclusionResponse(
                    sessionId,
                    $"Value type mismatch: expected '{canonicalValueType}', got '{metric.ValueType}'"));
                continue;
            }

            var numericValue = ExtractNumericValue(metric);
            points.Add(new MetricComparisonPointResponse(
                SessionId: sessionId,
                SessionDate: sessionBySessionId.TryGetValue(sessionId, out var session) ? session.StartedAt : null,
                Value: numericValue,
                Included: numericValue.HasValue,
                ExclusionReason: numericValue.HasValue ? null : "Metric has no numeric scalar value"));
        }

        var includedValues = points.Where(p => p.Included && p.Value.HasValue).Select(p => p.Value!.Value).ToList();

        if (includedValues.Count == 0)
        {
            return null;
        }

        var stats = new MetricStatisticsResponse(
            Mean: includedValues.Average(),
            Max: includedValues.Max(),
            SampleCount: includedValues.Count,
            InclusionRule: "Includes only sessions with matching metric code, unit, and value type that have a numeric scalar value.");

        return new CompatibleMetricGroupResponse(
            MetricCode: metricCode,
            MetricName: metricName ?? metricCode,
            Unit: canonicalUnit ?? string.Empty,
            ValueType: canonicalValueType ?? "Scalar",
            Statistics: stats,
            Points: points,
            Exclusions: exclusions);
    }

    private static double? ExtractNumericValue(Metric metric)
    {
        if (metric.ValueType == MetricValueType.Scalar)
        {
            return metric.ScalarValue;
        }

        if (metric.ValueType == MetricValueType.Bilateral)
        {
            if (metric.LeftValue.HasValue && metric.RightValue.HasValue)
            {
                return (metric.LeftValue.Value + metric.RightValue.Value) / 2.0;
            }

            return metric.LeftValue ?? metric.RightValue;
        }

        return null;
    }

    private static HistoryMetricResponse ToHistoryMetric(Metric metric) =>
        new(
            MetricId: metric.Id,
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