using KWinHub.Hub.Contracts.Reports;

namespace KWinHub.Hub.Reports;

public interface IReportService
{
    Task<ReportDetailResponse> GenerateAsync(
        Guid organizationId,
        Guid generatedByUserId,
        GenerateReportRequest request,
        CancellationToken cancellationToken = default);

    Task<ReportDetailResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReportResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<PlaybackAuthorizationResponse> RequestPlaybackAsync(
        Guid organizationId,
        Guid reportId,
        Guid assetId,
        CancellationToken cancellationToken = default);
}