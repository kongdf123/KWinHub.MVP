using KWinHub.Application.Platform;
using KWinHub.Domain.Audit;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class PlatformRepository : IPlatformRepository
{
    private readonly KWinHubDbContext _dbContext;

    public PlatformRepository(KWinHubDbContext dbContext)
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

    public async Task<IReadOnlyList<OrganizationMemberRow>> ListMembersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
                from membership in _dbContext.OrganizationMemberships.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking() on membership.UserId equals user.Id
                where membership.OrganizationId == organizationId
                orderby membership.CreatedAt
                select new
                {
                    user.Id,
                    user.Email,
                    user.DisplayName,
                    membership.Role,
                    membership.Status,
                    membership.CreatedAt
                })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new OrganizationMemberRow(
                r.Id,
                r.Email,
                r.DisplayName,
                r.Role,
                r.Status,
                r.CreatedAt))
            .ToList();
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
                u.CreatedAt,
                _dbContext.OrganizationMemberships.Count(m => m.UserId == u.Id)))
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

    private IQueryable<OrganizationProjection> Project(IQueryable<Organization> query) =>
        query.Select(o => new OrganizationProjection(
            o.Id,
            o.Name,
            o.Slug,
            o.Type,
            o.Status,
            o.CountryCode,
            o.CreatedAt,
            _dbContext.OrganizationMemberships.Count(m => m.OrganizationId == o.Id),
            _dbContext.Subjects.Count(s => s.OrganizationId == o.Id),
            _dbContext.MeasurementSessions.Count(s => s.OrganizationId == o.Id),
            _dbContext.Results.Count(r => r.OrganizationId == o.Id)));

    private static OrganizationListRow ToRow(OrganizationProjection row) =>
        new(
            row.Id,
            row.Name,
            row.Slug,
            row.Type.ToString(),
            row.Status.ToString(),
            row.CountryCode,
            row.CreatedAt,
            row.MemberCount,
            row.SubjectCount,
            row.SessionCount,
            row.ResultCount);

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
            row.OrganizationCount);

    private sealed record OrganizationProjection(
        Guid Id,
        string Name,
        string Slug,
        OrganizationType Type,
        OrganizationStatus Status,
        string? CountryCode,
        DateTimeOffset CreatedAt,
        int MemberCount,
        int SubjectCount,
        int SessionCount,
        int ResultCount);

    private sealed record UserProjection(
        Guid Id,
        string Email,
        string DisplayName,
        UserStatus Status,
        string? PlatformRole,
        bool EmailVerified,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset CreatedAt,
        int OrganizationCount);
}
