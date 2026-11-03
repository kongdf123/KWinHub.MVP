using KWinHub.Platform.Application.Platform;
using KWinHub.Platform.Audit;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.Platform.Organizations;
using KWinHub.Platform.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Platform.Infrastructure.Persistence.Repositories;

public class PlatformRepository : IPlatformRepository
{
    private readonly PlatformDbContext _dbContext;

    public PlatformRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountOrganizationsAsync(
        OrganizationStatus? status,
        OrganizationType? type,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Organizations.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(o => o.Type == type.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationListRow>> ListOrganizationsAsync(
        string? search,
        string? status,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Organizations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(o => o.Name.ToLower().Contains(term) || o.Slug.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<OrganizationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(type)
            && Enum.TryParse<OrganizationType>(type, true, out var parsedType))
        {
            query = query.Where(o => o.Type == parsedType);
        }

        var rows = await Project(query)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(ToRow).ToList();
    }

    public async Task<OrganizationListRow?> GetOrganizationRowAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await Project(_dbContext.Organizations.AsNoTracking().Where(o => o.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToRow(row);
    }

    public async Task<IReadOnlyList<UserSummary>> GetUserSummariesByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserSummary(u.Id, u.Email, u.DisplayName))
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization?> GetOrganizationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task SaveOrganizationAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountUsersAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(u => u.Status == parsedStatus);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserListRow>> ListUsersAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) || u.DisplayName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(u => u.Status == parsedStatus);
        }

        var rows = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProjection(
                u.Id,
                u.Email,
                u.DisplayName,
                u.Status,
                u.PlatformRole,
                u.EmailVerifiedAt != null,
                u.LastLoginAt,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return rows.Select(ToRow).ToList();
    }

    public async Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task SaveUserAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(log, cancellationToken);
    }

    public async Task SaveAuditAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> ListAuditLogsAsync(
        string? entityType,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        var limit = take <= 0 ? 100 : take;

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountGlobalProtocolsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalProtocols.CountAsync(cancellationToken);
    }

    public async Task<int> CountGlobalMetricsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalMetrics.CountAsync(cancellationToken);
    }

    // Workspace counts (members/subjects/sessions/results) are owned by the Hub
    // context. The Platform repository projects zeros here; PlatformService
    // overlays the authoritative values returned by IHubGateway.
    private IQueryable<OrganizationProjection> Project(IQueryable<Organization> query) =>
        query.Select(o => new OrganizationProjection(
            o.Id,
            o.Name,
            o.Slug,
            o.Type,
            o.Status,
            o.CountryCode,
            o.CreatedAt));

    private static OrganizationListRow ToRow(OrganizationProjection row) =>
        new(
            row.Id,
            row.Name,
            row.Slug,
            row.Type.ToString(),
            row.Status.ToString(),
            row.CountryCode,
            row.CreatedAt,
            MemberCount: 0,
            SubjectCount: 0,
            SessionCount: 0,
            ResultCount: 0);

    private static UserListRow ToRow(UserProjection row) =>
        new(
            row.Id,
            row.Email,
            row.DisplayName,
            row.Status.ToString(),
            row.PlatformRole,
            row.EmailVerified,
            row.LastLoginAt,
            row.CreatedAt,
            OrganizationCount: 0);

    private sealed record OrganizationProjection(
        Guid Id,
        string Name,
        string Slug,
        OrganizationType Type,
        OrganizationStatus Status,
        string? CountryCode,
        DateTimeOffset CreatedAt);

    private sealed record UserProjection(
        Guid Id,
        string Email,
        string DisplayName,
        UserStatus Status,
        string? PlatformRole,
        bool EmailVerified,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset CreatedAt);
}
