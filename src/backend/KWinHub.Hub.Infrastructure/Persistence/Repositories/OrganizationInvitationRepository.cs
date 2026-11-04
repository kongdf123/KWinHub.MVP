using KWinHub.Hub.Application.Organizations;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class OrganizationInvitationRepository : IOrganizationInvitationRepository
{
    private readonly HubDbContext _db;

    public OrganizationInvitationRepository(HubDbContext db)
    {
        _db = db;
    }

    public async Task<OrganizationInvitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<OrganizationInvitation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationInvitations
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationInvitation>> ListByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.OrganizationInvitations
            .AsNoTracking()
            .Where(i => i.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(i => i.Status == status);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        OrganizationInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        await _db.OrganizationInvitations.AddAsync(invitation, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        OrganizationInvitation invitation,
        CancellationToken cancellationToken = default)
    {
        _db.OrganizationInvitations.Update(invitation);
        await _db.SaveChangesAsync(cancellationToken);
    }
}