using KWinHub.Application.Subscriptions;
using KWinHub.Domain.Subscriptions;
using KWinHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly KWinHubDbContext _db;

    public SubscriptionRepository(KWinHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SubscriptionPlan>> ListPlansAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .Include(p => p.Features)
            .Include(p => p.Quotas)
            .OrderBy(p => p.Code)
            .ThenBy(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .Include(p => p.Features)
            .Include(p => p.Quotas)
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
    }

    public async Task AddPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
        await _db.SubscriptionPlans.AddAsync(plan, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
        _db.SubscriptionPlans.Update(plan);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationSubscription?> GetActiveSubscriptionAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationSubscriptions
            .Where(s => s.OrganizationId == organizationId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationSubscription>> ListSubscriptionsAsync(
        Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.OrganizationSubscriptions.AsQueryable();
        if (organizationId.HasValue)
        {
            query = query.Where(s => s.OrganizationId == organizationId.Value);
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddSubscriptionAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _db.OrganizationSubscriptions.AddAsync(subscription, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSubscriptionAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default)
    {
        _db.OrganizationSubscriptions.Update(subscription);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FeatureDefinition>> ListFeatureDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FeatureDefinitions.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationFeatureOverride>> GetFeatureOverridesAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationFeatureOverrides
            .Where(o => o.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationFeatureOverride?> GetFeatureOverrideAsync(
        Guid organizationId, string featureKey, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationFeatureOverrides
            .Where(o => o.OrganizationId == organizationId && o.FeatureKey == featureKey)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddFeatureOverrideAsync(OrganizationFeatureOverride overrideEntity, CancellationToken cancellationToken = default)
    {
        await _db.OrganizationFeatureOverrides.AddAsync(overrideEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationQuotaOverride>> GetQuotaOverridesAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationQuotaOverrides
            .Where(o => o.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationQuotaOverride?> GetQuotaOverrideAsync(
        Guid organizationId, string quotaKey, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationQuotaOverrides
            .Where(o => o.OrganizationId == organizationId && o.QuotaKey == quotaKey)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddQuotaOverrideAsync(OrganizationQuotaOverride overrideEntity, CancellationToken cancellationToken = default)
    {
        await _db.OrganizationQuotaOverrides.AddAsync(overrideEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAuditEventAsync(SubscriptionAuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        await _db.SubscriptionAuditEvents.AddAsync(auditEvent, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionAuditEvent>> ListAuditEventsAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionAuditEvents
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}
