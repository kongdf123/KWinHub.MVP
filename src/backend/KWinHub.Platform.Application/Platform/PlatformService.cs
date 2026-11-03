using KWinHub.Platform.Application.CrossContext;
using KWinHub.Platform.Application.Organizations;
using KWinHub.Platform.Contracts.Platform;
using KWinHub.Platform.Audit;
using KWinHub.Platform.Organizations;
using KWinHub.Platform.Users;

namespace KWinHub.Platform.Application.Platform;

public class PlatformService : IPlatformService
{
    private const int RecentOrganizationCount = 5;
    private const int RecentAuditLogCount = 10;
    private const int DefaultAuditLogTake = 100;

    private readonly IPlatformRepository _repository;
    private readonly IHubGateway _hubGateway;
    private readonly IOrganizationEventOutbox _eventOutbox;

    public PlatformService(IPlatformRepository repository, IHubGateway hubGateway, IOrganizationEventOutbox eventOutbox)
    {
        _repository = repository;
        _hubGateway = hubGateway;
        _eventOutbox = eventOutbox;
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

        var organizations = await ListOrganizationsAsync(null, null, null, cancellationToken);
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
            RecentOrganizations: organizations.Take(RecentOrganizationCount).ToList(),
            RecentAuditLogs: auditLogs.Select(ToResponse).ToList());
    }

    public async Task<IReadOnlyList<PlatformOrganizationResponse>> ListOrganizationsAsync(
        string? search,
        string? status,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.ListOrganizationsAsync(search, status, type, cancellationToken);
        var stats = await _hubGateway.GetOrganizationStatsAsync(
            rows.Select(r => r.Id).ToList(),
            cancellationToken);

        return rows.Select(row => ToResponse(ApplyStats(row, stats))).ToList();
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

        // Memberships are Hub-owned; user identities are Platform-owned. Merge the
        // two in the application layer rather than joining across contexts.
        var memberships = await _hubGateway.ListMembershipsAsync(id, cancellationToken);
        var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
        var users = await _repository.GetUserSummariesByIdsAsync(userIds, cancellationToken);
        var usersById = users.ToDictionary(u => u.Id);

        var members = memberships
            .Select(m =>
            {
                usersById.TryGetValue(m.UserId, out var user);
                return new PlatformOrganizationMemberResponse(
                    m.UserId,
                    user?.Email ?? string.Empty,
                    user?.DisplayName ?? string.Empty,
                    m.Role,
                    m.Status,
                    m.JoinedAt);
            })
            .ToList();

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
            Members: members);
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

        var eventType = normalizedAction == "suspend"
            ? OrganizationLifecycleEventTypes.Suspended
            : OrganizationLifecycleEventTypes.Reactivated;
        var latestVersion = await _eventOutbox.GetLatestVersionAsync(organization.Id, cancellationToken);
        var nextVersion = latestVersion + 1;
        var payload = new OrganizationLifecycleEventPayload(
            organization.Id,
            eventType,
            nextVersion,
            organization.Status.ToString(),
            DateTimeOffset.UtcNow);
        await _eventOutbox.AddAsync(
            new OrganizationLifecycleEvent(
                organization.Id,
                eventType,
                nextVersion,
                System.Text.Json.JsonSerializer.Serialize(payload)),
            cancellationToken);

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
        var membershipCounts = await _hubGateway.GetUserMembershipCountsAsync(
            rows.Select(r => r.Id).ToList(),
            cancellationToken);

        return rows
            .Select(row => ToResponse(row with
            {
                OrganizationCount = membershipCounts.TryGetValue(row.Id, out var count) ? count : 0
            }))
            .ToList();
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

    private static OrganizationListRow ApplyStats(
        OrganizationListRow row,
        IReadOnlyList<HubOrganizationStats> stats)
    {
        var stat = stats.FirstOrDefault(s => s.OrganizationId == row.Id);
        return row with
        {
            MemberCount = stat?.MemberCount ?? 0,
            SubjectCount = stat?.SubjectCount ?? 0,
            SessionCount = stat?.SessionCount ?? 0,
            ResultCount = stat?.ResultCount ?? 0
        };
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