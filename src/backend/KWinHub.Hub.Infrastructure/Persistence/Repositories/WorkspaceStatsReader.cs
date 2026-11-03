using KWinHub.Hub.Application.CrossContext;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

/// <summary>
/// Reads Hub-owned workspace counters for the Platform dashboard. Every table
/// queried here lives in the hub schema; no platform tables are touched.
/// </summary>
public class WorkspaceStatsReader : IWorkspaceStatsReader
{
    private readonly HubDbContext _db;

    public WorkspaceStatsReader(HubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrganizationWorkspaceStats>> GetOrganizationStatsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken = default)
    {
        if (organizationIds.Count == 0)
        {
            return [];
        }

        var memberCounts = await CountByOrganizationAsync(
            _db.OrganizationMemberships.Where(m => organizationIds.Contains(m.OrganizationId)).Select(m => m.OrganizationId),
            cancellationToken);
        var subjectCounts = await CountByOrganizationAsync(
            _db.Subjects.Where(s => organizationIds.Contains(s.OrganizationId)).Select(s => s.OrganizationId),
            cancellationToken);
        var sessionCounts = await CountByOrganizationAsync(
            _db.MeasurementSessions.Where(s => organizationIds.Contains(s.OrganizationId)).Select(s => s.OrganizationId),
            cancellationToken);
        var resultCounts = await CountByOrganizationAsync(
            _db.Results.Where(r => organizationIds.Contains(r.OrganizationId)).Select(r => r.OrganizationId),
            cancellationToken);

        return organizationIds
            .Select(id => new OrganizationWorkspaceStats(
                id,
                memberCounts.GetValueOrDefault(id),
                subjectCounts.GetValueOrDefault(id),
                sessionCounts.GetValueOrDefault(id),
                resultCounts.GetValueOrDefault(id)))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountMembershipsByUserAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => userIds.Contains(m.UserId))
            .GroupBy(m => m.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
    }

    private static async Task<Dictionary<Guid, int>> CountByOrganizationAsync(
        IQueryable<Guid> organizationIds,
        CancellationToken cancellationToken)
    {
        return await organizationIds
            .GroupBy(id => id)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId, x => x.Count, cancellationToken);
    }
}
