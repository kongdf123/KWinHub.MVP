using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.Plans;
using KWinHub.Hub.Plans;

namespace KWinHub.Hub.Application.Plans;

public class AssessmentPlanService : IAssessmentPlanService
{
    private readonly IAssessmentPlanRepository _planRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IResultRepository _resultRepository;

    public AssessmentPlanService(
        IAssessmentPlanRepository planRepository,
        ISessionRepository sessionRepository,
        IResultRepository resultRepository)
    {
        _planRepository = planRepository;
        _sessionRepository = sessionRepository;
        _resultRepository = resultRepository;
    }

    public async Task<AssessmentPlanResponse> CreateAsync(
        Guid organizationId,
        Guid userId,
        CreateAssessmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = new AssessmentPlan(
            organizationId,
            request.Name,
            request.Code,
            userId,
            request.ProtocolId,
            request.ProtocolCode,
            request.Description);

        var items = request.Items.Select(i => new AssessmentPlanItem(
            plan.Id,
            i.MetricCode,
            i.MetricName,
            i.Unit,
            i.ValueType,
            i.Required,
            i.MinValue,
            i.MaxValue)).ToList();

        plan.SetItems(items);

        await _planRepository.AddAsync(plan, cancellationToken);
        return ToResponse(plan);
    }

    public async Task<AssessmentPlanResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, id, cancellationToken);
        return plan is null ? null : ToResponse(plan);
    }

    public async Task<IReadOnlyList<AssessmentPlanResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.ListByOrganizationAsync(organizationId, cancellationToken);
        return plans.Select(ToResponse).ToList();
    }

    public async Task<AssessmentPlanResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateAssessmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, id, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        plan.UpdateDetails(request.Name, request.Description, request.ProtocolId, request.ProtocolCode);

        var items = request.Items.Select(i => new AssessmentPlanItem(
            plan.Id,
            i.MetricCode,
            i.MetricName,
            i.Unit,
            i.ValueType,
            i.Required,
            i.MinValue,
            i.MaxValue)).ToList();

        plan.SetItems(items);

        await _planRepository.UpdateAsync(plan, cancellationToken);
        return ToResponse(plan);
    }

    public async Task<AssessmentPlanResponse?> PublishAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, id, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        plan.Publish();
        await _planRepository.UpdateAsync(plan, cancellationToken);
        return ToResponse(plan);
    }

    public async Task<AssessmentPlanResponse?> RetireAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, id, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        plan.Retire();
        await _planRepository.UpdateAsync(plan, cancellationToken);
        return ToResponse(plan);
    }

    public async Task<AssessmentPlanResponse?> CreateNewVersionAsync(
        Guid organizationId,
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, id, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        var newVersion = plan.CreateNewVersion(userId);
        await _planRepository.AddAsync(newVersion, cancellationToken);
        return ToResponse(newVersion);
    }

    public async Task<PlanCompletenessResponse> CheckCompletenessAsync(
        Guid organizationId,
        Guid planId,
        Guid subjectId,
        IReadOnlyList<Guid> selectedSessionIds,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, planId, cancellationToken);
        if (plan is null)
        {
            return new PlanCompletenessResponse(
                planId,
                0,
                subjectId,
                selectedSessionIds,
                false,
                new[] { $"Plan {planId} not found" },
                Array.Empty<CandidateSessionResponse>());
        }

        var requiredItems = plan.Items.Where(i => i.Required).ToList();

        var sessions = await _sessionRepository.ListBySubjectFilteredAsync(
            organizationId,
            subjectId,
            plan.ProtocolId,
            cancellationToken: cancellationToken);

        var candidateSessions = sessions.Select(s => new { Session = s, SessionId = s.Id }).ToList();
        var validSelectedIds = selectedSessionIds
            .Where(id => candidateSessions.Any(c => c.SessionId == id))
            .ToList();

        var results = await _resultRepository.ListBySessionsAsync(organizationId, validSelectedIds, cancellationToken);
        var resultsBySession = results.ToDictionary(r => r.SessionId);

        var candidateResponses = new List<CandidateSessionResponse>();
        var blockingReasons = new List<string>();

        foreach (var candidate in candidateSessions)
        {
            var session = candidate.Session;
            var hasResult = resultsBySession.TryGetValue(session.Id, out var result);

            var missingRequired = new List<string>();
            var incompatible = new List<string>();

            foreach (var item in requiredItems)
            {
                if (!hasResult || result is null)
                {
                    missingRequired.Add(item.MetricCode);
                    continue;
                }

                var metric = result.Metrics.FirstOrDefault(m => m.MetricCode == item.MetricCode);
                if (metric is null)
                {
                    missingRequired.Add(item.MetricCode);
                    continue;
                }

                if (metric.Unit != item.Unit)
                {
                    incompatible.Add($"{item.MetricCode}: unit mismatch (expected '{item.Unit}', got '{metric.Unit}')");
                    continue;
                }

                if (metric.ValueType.ToString() != item.ValueType)
                {
                    incompatible.Add($"{item.MetricCode}: value type mismatch (expected '{item.ValueType}', got '{metric.ValueType}')");
                }
            }

            var allRequiredPresent = missingRequired.Count == 0 && incompatible.Count == 0;
            candidateResponses.Add(new CandidateSessionResponse(
                SessionId: session.Id,
                SubjectId: session.SubjectId,
                ProtocolId: session.ProtocolId,
                ProtocolVersion: session.ProtocolVersion,
                Status: session.Status.ToString(),
                StartedAt: session.StartedAt,
                HasResult: hasResult,
                AllRequiredMetricsPresent: allRequiredPresent,
                MissingRequiredMetrics: missingRequired,
                IncompatibleMetrics: incompatible));
        }

        var selectedCandidates = candidateResponses.Where(c => validSelectedIds.Contains(c.SessionId)).ToList();

        if (validSelectedIds.Count == 0)
        {
            blockingReasons.Add("No valid sessions selected for this subject and plan.");
        }
        else
        {
            foreach (var c in selectedCandidates)
            {
                if (!c.HasResult)
                {
                    blockingReasons.Add($"Session {c.SessionId} has no published result.");
                }
                foreach (var missing in c.MissingRequiredMetrics)
                {
                    blockingReasons.Add($"Session {c.SessionId} is missing required metric: {missing}.");
                }
                foreach (var inc in c.IncompatibleMetrics)
                {
                    blockingReasons.Add($"Session {c.SessionId} incompatible: {inc}.");
                }
            }
        }

        return new PlanCompletenessResponse(
            plan.Id,
            plan.Version,
            subjectId,
            validSelectedIds,
            blockingReasons.Count == 0,
            blockingReasons,
            candidateResponses);
    }

    private static AssessmentPlanResponse ToResponse(AssessmentPlan plan) =>
        new(
            plan.Id,
            plan.OrganizationId,
            plan.Name,
            plan.Code,
            plan.Description,
            plan.Version,
            plan.Status.ToString(),
            plan.ProtocolId,
            plan.ProtocolCode,
            plan.CreatedByUserId,
            plan.Items.Select(ToItemResponse).ToList(),
            plan.CreatedAt,
            plan.UpdatedAt);

    private static AssessmentPlanItemResponse ToItemResponse(AssessmentPlanItem item) =>
        new(
            item.Id,
            item.PlanId,
            item.MetricCode,
            item.MetricName,
            item.Unit,
            item.ValueType,
            item.Required,
            item.MinValue,
            item.MaxValue,
            item.SortOrder);
}