using KWinHub.Contracts.Platform;
using KWinHub.Domain.Audit;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Users;

namespace KWinHub.Application.Platform;

public class PlatformService : IPlatformService
{
    private const int RecentOrganizationCount = 5;
    private const int RecentAuditLogCount = 10;
    private const int DefaultAuditLogTake = 100;

    private readonly IPlatformRepository _repository;

    public PlatformService(IPlatformRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlatformDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalOrganizations = await _repository.CountOrganizationsAsync(null, null, cancellationToken);
        var activeOrganizations = await _repository.CountOrganizationsAsync(OrganizationStatus.Active, null, cancellationToken);
        var suspendedOrganizations = await _repository.CountOrganizationsAsync(OrganizationStatus.Suspended, null, cancellationToken);
        var individualOrganizations = await _repository.CountOrganizationsAsync(null, OrganizationType.Individual, cancellationToken);

        var totalUsers = await _repository.CountUsersAsync(null, cancellationToken);
        var activeUsers = await _repository.CountUsersAsync(UserStatus.Active.ToString(), cancellationToken);
        var pendingUsers = await _repository.CountUsersAsync(UserStatus.PendingEmailVerification.ToString(), cancellationToken);

        var totalProtocols = await _repository.CountGlobalProtocolsAsync(cancellationToken);
        var totalMetrics = await _repository.CountGlobalMetricsAsync(cancellationToken);

        var organizations = await _repository.ListOrganizationsAsync(null, null, null, cancellationToken);
        var auditLogs = await _repository.ListAuditLogsAsync(null, RecentAuditLogCount, cancellationToken);

        return new PlatformDashboardResponse(
            TotalOrganizations: totalOrganizations,
            ActiveOrganizations: activeOrganizations,
            SuspendedOrganizations: suspendedOrganizations,
            IndividualOrganizations: individualOrganizations,
            TotalUsers: totalUsers,
            ActiveUsers: activeUsers,
            PendingVerificationUsers: pendingUsers,
            TotalProtocolDefinitions: totalProtocols,
            TotalMetrics: totalMetrics,
            RecentOrganizations: organizations.Take(RecentOrganizationCount).Select(ToResponse).ToList(),
            RecentAuditLogs: auditLogs.Select(ToResponse).ToList());
    }

    public async Task<IReadOnlyList<PlatformOrganizationResponse>> ListOrganizationsAsync(
        string? search,
        string? status,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.ListOrganizationsAsync(search, status, type, cancellationToken);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<PlatformOrganizationDetailResponse?> GetOrganizationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _repository.GetOrganizationRowAsync(id, cancellationToken);
        if (row is null)
        {
            return null;
        }

        var organization = await _repository.GetOrganizationAsync(id, cancellationToken);
        var members = await _repository.ListMembersAsync(id, cancellationToken);

        return new PlatformOrganizationDetailResponse(
            Id: row.Id,
            Name: row.Name,
            Slug: row.Slug,
            Type: row.Type,
            Status: row.Status,
            CountryCode: row.CountryCode,
            Region: organization?.Region,
            Timezone: organization?.Timezone,
            Locale: organization?.Locale,
            CreatedByUserId: organization?.CreatedByUserId,
            CreatedAt: row.CreatedAt,
            Members: members.Select(m => new PlatformOrganizationMemberResponse(
                m.UserId,
                m.Email,
                m.DisplayName,
                m.Role,
                m.Status,
                m.JoinedAt)).ToList());
    }

    public async Task<bool> ChangeOrganizationStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = (action ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedAction is not ("suspend" or "reactivate"))
        {
            throw new ArgumentException("Action must be 'suspend' or 'reactivate'.", nameof(action));
        }

        var organization = await _repository.GetOrganizationAsync(id, cancellationToken);
        if (organization is null)
        {
            return false;
        }

        if (normalizedAction == "suspend")
        {
            organization.Suspend();
        }
        else
        {
            organization.Reactivate();
        }

        await _repository.SaveOrganizationAsync(cancellationToken);

        await RecordAuditAsync(
            action: normalizedAction == "suspend" ? "organization.suspended" : "organization.reactivated",
            entityType: "Organization",
            entityId: organization.Id.ToString(),
            summary: normalizedAction == "suspend"
                ? $"Organization '{organization.Name}' suspended."
                : $"Organization '{organization.Name}' reactivated.",
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            ipAddress: ipAddress,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<PlatformUserResponse>> ListUsersAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.ListUsersAsync(search, status, cancellationToken);
        return rows.Select(ToResponse).ToList();
    }

    public async Task<bool> ChangeUserStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = (action ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedAction is not ("suspend" or "activate" or "grant-admin" or "revoke-admin"))
        {
            throw new ArgumentException(
                "Action must be 'suspend', 'activate', 'grant-admin' or 'revoke-admin'.",
                nameof(action));
        }

        var user = await _repository.GetUserAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        string auditAction;
        string summary;

        switch (normalizedAction)
        {
            case "suspend":
                user.SetStatus(UserStatus.Suspended);
                auditAction = "user.suspended";
                summary = $"User '{user.Email}' suspended.";
                break;
            case "activate":
                user.SetStatus(UserStatus.Active);
                auditAction = "user.activated";
                summary = $"User '{user.Email}' activated.";
                break;
            case "grant-admin":
                user.GrantPlatformAdmin();
                auditAction = "user.granted-admin";
                summary = $"User '{user.Email}' granted platform administrator role.";
                break;
            default:
                user.RevokePlatformAdmin();
                auditAction = "user.revoked-admin";
                summary = $"User '{user.Email}' platform administrator role revoked.";
                break;
        }

        await _repository.SaveUserAsync(cancellationToken);

        await RecordAuditAsync(
            action: auditAction,
            entityType: "User",
            entityId: user.Id.ToString(),
            summary: summary,
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            ipAddress: ipAddress,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<AuditLogResponse>> ListAuditLogsAsync(
        string? entityType,
        int take,
        CancellationToken cancellationToken = default)
    {
        var limit = take <= 0 ? DefaultAuditLogTake : take;
        var logs = await _repository.ListAuditLogsAsync(entityType, limit, cancellationToken);
        return logs.Select(ToResponse).ToList();
    }

    public async Task<Guid?> RecordAuditAsync(
        string action,
        string entityType,
        string? entityId,
        string summary,
        Guid? actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(
            action: action,
            entityType: entityType,
            entityId: entityId,
            summary: summary,
            actorUserId: actorUserId,
            actorEmail: actorEmail,
            ipAddress: ipAddress);

        await _repository.AddAuditLogAsync(log, cancellationToken);
        await _repository.SaveAuditAsync(cancellationToken);

        return log.Id;
    }

    private static PlatformOrganizationResponse ToResponse(OrganizationListRow row) =>
        new(
            Id: row.Id,
            Name: row.Name,
            Slug: row.Slug,
            Type: row.Type,
            Status: row.Status,
            CountryCode: row.CountryCode,
            CreatedAt: row.CreatedAt,
            MemberCount: row.MemberCount,
            SubjectCount: row.SubjectCount,
            SessionCount: row.SessionCount,
            ResultCount: row.ResultCount);

    private static PlatformUserResponse ToResponse(UserListRow row) =>
        new(
            Id: row.Id,
            Email: row.Email,
            DisplayName: row.DisplayName,
            Status: row.Status,
            PlatformRole: row.PlatformRole,
            EmailVerified: row.EmailVerified,
            LastLoginAt: row.LastLoginAt,
            CreatedAt: row.CreatedAt,
            OrganizationCount: row.OrganizationCount);

    private static AuditLogResponse ToResponse(AuditLog log) =>
        new(
            Id: log.Id,
            ActorUserId: log.ActorUserId,
            ActorEmail: log.ActorEmail,
            Action: log.Action,
            EntityType: log.EntityType,
            EntityId: log.EntityId,
            Summary: log.Summary,
            MetadataJson: log.MetadataJson,
            CreatedAt: log.CreatedAt);
}
