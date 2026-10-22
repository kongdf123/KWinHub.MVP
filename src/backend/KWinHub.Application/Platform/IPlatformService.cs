using KWinHub.Contracts.Platform;

namespace KWinHub.Application.Platform;

public interface IPlatformService
{
    Task<PlatformDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformOrganizationResponse>> ListOrganizationsAsync(
        string? search,
        string? status,
        string? type,
        CancellationToken cancellationToken = default);

    Task<PlatformOrganizationDetailResponse?> GetOrganizationAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeOrganizationStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformUserResponse>> ListUsersAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogResponse>> ListAuditLogsAsync(
        string? entityType,
        int take,
        CancellationToken cancellationToken = default);

    Task<Guid?> RecordAuditAsync(
        string action,
        string entityType,
        string? entityId,
        string summary,
        Guid? actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
