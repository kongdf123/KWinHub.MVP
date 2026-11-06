using KWinHub.Hub.Assets;
using KWinHub.Hub.Application.Plans;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.Reports;
using KWinHub.Hub.Plans;
using KWinHub.Hub.Reports;

namespace KWinHub.Hub.Application.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IAssessmentPlanRepository _planRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IResultRepository _resultRepository;
    private readonly ITrialRepository _trialRepository;
    private readonly IAssetRepository _assetRepository;

    public ReportService(
        IReportRepository reportRepository,
        IAssessmentPlanRepository planRepository,
        ISessionRepository sessionRepository,
        IResultRepository resultRepository,
        ITrialRepository trialRepository,
        IAssetRepository assetRepository)
    {
        _reportRepository = reportRepository;
        _planRepository = planRepository;
        _sessionRepository = sessionRepository;
        _resultRepository = resultRepository;
        _trialRepository = trialRepository;
        _assetRepository = assetRepository;
    }

    public async Task<ReportDetailResponse> GenerateAsync(
        Guid organizationId,
        Guid generatedByUserId,
        GenerateReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(organizationId, request.PlanId, cancellationToken);
        if (plan is null)
        {
            throw new InvalidOperationException($"Plan {request.PlanId} not found.");
        }

        var sessions = await _sessionRepository.ListBySubjectFilteredAsync(
            organizationId,
            request.SubjectId,
            plan.ProtocolId,
            cancellationToken: cancellationToken);

        var validSessionIds = request.SessionIds
            .Where(id => sessions.Any(s => s.Id == id))
            .ToList();

        if (validSessionIds.Count == 0)
        {
            throw new InvalidOperationException("No valid sessions selected for this subject and plan.");
        }

        var validSessions = sessions.Where(s => validSessionIds.Contains(s.Id)).ToList();

        var results = await _resultRepository.ListBySessionsAsync(organizationId, validSessionIds, cancellationToken);
        var resultsBySession = results.GroupBy(r => r.SessionId).ToDictionary(g => g.Key, g => g.ToList());

        var trialsBySession = new Dictionary<Guid, IReadOnlyList<KWinHub.Hub.Sessions.Trial>>();
        foreach (var sessionId in validSessionIds)
        {
            trialsBySession[sessionId] = await _trialRepository.ListBySessionAsync(organizationId, sessionId, cancellationToken);
        }

        var assetsBySession = new Dictionary<Guid, IReadOnlyList<KWinHub.Hub.Assets.DataAsset>>();
        foreach (var sessionId in validSessionIds)
        {
            assetsBySession[sessionId] = await _assetRepository.GetBySessionAsync(organizationId, sessionId, cancellationToken);
        }

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"{plan.Name} — {request.SubjectId}"
            : request.Title;

        var report = new Report(
            organizationId,
            plan.Id,
            plan.Code,
            plan.Version,
            plan.Name,
            request.SubjectId,
            generatedByUserId,
            title,
            request.Notes);

        var orderedItems = plan.Items.OrderBy(i => i.SortOrder).ToList();

        var allRequiredPresent = true;

        foreach (var session in validSessions)
        {
            var sessionResults = resultsBySession.TryGetValue(session.Id, out var r) ? r : new List<KWinHub.Hub.Results.Result>();
            var primaryResult = sessionResults.FirstOrDefault(res => res.Status == "Published") ?? sessionResults.FirstOrDefault();

            foreach (var item in orderedItems)
            {
                double? value = null;
                Guid? resultId = primaryResult?.Id;
                Guid? trialId = primaryResult?.TrialId;

                if (primaryResult is not null)
                {
                    var metric = primaryResult.Metrics.FirstOrDefault(m =>
                        m.MetricCode == item.MetricCode && m.Unit == item.Unit);

                    if (metric is not null)
                    {
                        value = metric.Value;
                    }
                }

                if (item.Required && value is null)
                {
                    allRequiredPresent = false;
                }

                report.AddMetricValue(new ReportMetricValue(
                    organizationId,
                    report.Id,
                    session.Id,
                    trialId,
                    resultId,
                    session.StartedAt,
                    item.MetricCode,
                    item.MetricName,
                    item.Unit,
                    value,
                    item.Required,
                    item.SortOrder));
            }

            var sessionAssets = assetsBySession.TryGetValue(session.Id, out var a) ? a : Array.Empty<KWinHub.Hub.Assets.DataAsset>();
            foreach (var asset in sessionAssets)
            {
                report.AddAssetState(new ReportAssetState(
                    organizationId,
                    report.Id,
                    asset.Id,
                    session.Id,
                    asset.TrialId,
                    asset.AssetType,
                    asset.Status.ToString(),
                    asset.MediaProviderId,
                    asset.PlaybackUrl,
                    asset.FailureReason,
                    asset.ProcessedAt));
            }
        }

        if (allRequiredPresent)
        {
            report.MarkComplete();
        }

        await _reportRepository.AddAsync(report, cancellationToken);

        return ToDetailResponse(report);
    }

    public async Task<ReportDetailResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(organizationId, id, cancellationToken);
        return report is null ? null : ToDetailResponse(report);
    }

    public async Task<IReadOnlyList<ReportResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var reports = await _reportRepository.ListByOrganizationAsync(organizationId, cancellationToken);
        return reports.Select(ToResponse).ToList();
    }

    public async Task<PlaybackAuthorizationResponse> RequestPlaybackAsync(
        Guid organizationId,
        Guid reportId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(organizationId, reportId, cancellationToken);
        if (report is null)
        {
            return new PlaybackAuthorizationResponse(assetId, false, null, null, "Report not found.");
        }

        var assetState = report.AssetStates.FirstOrDefault(a => a.AssetId == assetId);
        if (assetState is null)
        {
            return new PlaybackAuthorizationResponse(assetId, false, null, null, "Asset is not part of this report.");
        }

        if (assetState.Status != KWinHub.Hub.Assets.DataAssetStatus.Ready.ToString())
        {
            return new PlaybackAuthorizationResponse(
                assetId,
                false,
                null,
                null,
                $"Asset is not ready (status: {assetState.Status}).");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10).ToString("o");
        return new PlaybackAuthorizationResponse(
            assetId,
            true,
            assetState.PlaybackUrl,
            expiresAt,
            null);
    }

    private static ReportResponse ToResponse(Report report) =>
        new(
            report.Id,
            report.OrganizationId,
            report.PlanId,
            report.PlanCode,
            report.PlanVersion,
            report.PlanName,
            report.SubjectId,
            report.GeneratedByUserId,
            report.Title,
            report.Status.ToString(),
            report.Notes,
            report.GeneratedAt,
            report.MetricValues.Count,
            report.AssetStates.Count);

    private static ReportDetailResponse ToDetailResponse(Report report) =>
        new(
            report.Id,
            report.OrganizationId,
            report.PlanId,
            report.PlanCode,
            report.PlanVersion,
            report.PlanName,
            report.SubjectId,
            report.GeneratedByUserId,
            report.Title,
            report.Status.ToString(),
            report.Notes,
            report.GeneratedAt,
            report.MetricValues.Select(ToMetricResponse).ToList(),
            report.AssetStates.Select(ToAssetResponse).ToList());

    private static ReportMetricValueResponse ToMetricResponse(ReportMetricValue v) =>
        new(
            v.Id,
            v.SessionId,
            v.TrialId,
            v.ResultId,
            v.SessionStartedAt,
            v.MetricCode,
            v.MetricName,
            v.Unit,
            v.Value,
            v.Required,
            v.PlanItemSortOrder);

    private static ReportAssetStateResponse ToAssetResponse(ReportAssetState a) =>
        new(
            a.Id,
            a.AssetId,
            a.SessionId,
            a.TrialId,
            a.AssetType,
            a.Status,
            a.MediaProviderId,
            a.PlaybackUrl,
            a.FailureReason,
            a.ProcessedAt,
            PlaybackAuthorized: a.Status == KWinHub.Hub.Assets.DataAssetStatus.Ready.ToString());
}