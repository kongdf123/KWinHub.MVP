using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Groups;
using KWinHub.Hub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly HubDbContext _dbContext;

    public GroupRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Groups
            .Include(g => g.Memberships)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

    public async Task<List<Group>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? groupType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(g =>
                g.Name.ToLower().Contains(normalized) ||
                (g.Description != null && g.Description.ToLower().Contains(normalized)) ||
                (g.GroupType != null && g.GroupType.ToLower().Contains(normalized)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(g => g.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(groupType))
        {
            query = query.Where(g => g.GroupType == groupType);
        }

        return await query
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        await _dbContext.Groups.AddAsync(group, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GroupMemberships
            .CountAsync(m => m.GroupId == groupId, cancellationToken);
    }

    public async Task AddMembershipAsync(GroupMembership membership, CancellationToken cancellationToken = default)
    {
        await _dbContext.GroupMemberships.AddAsync(membership, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveMembershipAsync(Guid groupId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.SubjectId == subjectId, cancellationToken);
        if (membership is null)
        {
            return false;
        }

        _dbContext.GroupMemberships.Remove(membership);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}