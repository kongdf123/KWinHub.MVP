using KWinHub.Hub.Application.Teams;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Teams;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly HubDbContext _dbContext;

    public TeamRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Team?> GetByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .Include(t => t.Memberships)
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
    }

    public async Task<List<Team>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Teams
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Name.ToLower().Contains(normalized) ||
                (t.Description != null && t.Description.ToLower().Contains(normalized)) ||
                (t.Season != null && t.Season.ToLower().Contains(normalized)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _dbContext.Teams.AddAsync(team, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TeamMemberships
            .CountAsync(m => m.TeamId == teamId, cancellationToken);
    }

    public async Task AddMembershipAsync(TeamMembership membership, CancellationToken cancellationToken = default)
    {
        await _dbContext.TeamMemberships.AddAsync(membership, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveMembershipAsync(Guid teamId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.TeamMemberships
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.SubjectId == subjectId, cancellationToken);
        if (membership is null)
        {
            return false;
        }

        _dbContext.TeamMemberships.Remove(membership);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}