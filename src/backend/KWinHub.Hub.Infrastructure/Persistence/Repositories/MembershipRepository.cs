using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly HubDbContext _db;

    public MembershipRepository(HubDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships.AnyAsync(
            m => m.UserId == userId
                 && m.OrganizationId == organizationId
                 && m.Status == OrganizationMembership.StatusActive,
            cancellationToken);
    }

    public async Task<OrganizationMembership?> GetAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken = default)
    {
        await _db.OrganizationMemberships.AddAsync(membership, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken = default)
    {
        _db.OrganizationMemberships.Update(membership);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationMembership?> GetPrimaryByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.Status == OrganizationMembership.StatusActive)
            .OrderBy(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationMembership>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships.AnyAsync(
            m => m.UserId == userId && m.OrganizationId == organizationId,
            cancellationToken);
    }
}

public class OrganizationStatusRepository : IOrganizationStatusRepository
{
    private readonly HubDbContext _db;

    public OrganizationStatusRepository(HubDbContext db)
    {
        _db = db;
    }

    public async Task<OrganizationStatus?> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId, cancellationToken);
    }

    public async Task UpsertAsync(
        OrganizationStatus status,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.OrganizationStatuses
            .FirstOrDefaultAsync(s => s.OrganizationId == status.OrganizationId, cancellationToken);

        if (existing is null)
        {
            await _db.OrganizationStatuses.AddAsync(status, cancellationToken);
        }
        else
        {
            existing.ApplyLifecycleEvent(status.Status, status.Version);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsOrganizationActiveAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var status = await GetAsync(organizationId, cancellationToken);
        return status?.IsActive ?? true;
    }
}

public class OrganizationEventConsumptionRepository : IOrganizationEventConsumptionRepository
{
    private readonly HubDbContext _db;

    public OrganizationEventConsumptionRepository(HubDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsConsumedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationEventConsumptions.AnyAsync(
            c => c.EventId == eventId, cancellationToken);
    }

    public async Task MarkConsumedAsync(
        OrganizationEventConsumption consumption,
        CancellationToken cancellationToken = default)
    {
        await _db.OrganizationEventConsumptions.AddAsync(consumption, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}