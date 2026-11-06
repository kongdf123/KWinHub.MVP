using KWinHub.Hub.Application.Plans;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Plans;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class AssessmentPlanRepository : IAssessmentPlanRepository
{
    private readonly HubDbContext _dbContext;

    public AssessmentPlanRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentPlan?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssessmentPlans
            .AsNoTracking()
            .Include(p => p.Items)
            .SingleOrDefaultAsync(
                p => p.Id == id && p.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AssessmentPlan>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssessmentPlans
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.Code)
            .ThenByDescending(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssessmentPlan>> ListByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssessmentPlans
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.OrganizationId == organizationId && p.Code == code)
            .OrderByDescending(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<AssessmentPlan?> GetPublishedByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssessmentPlans
            .AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.OrganizationId == organizationId && p.Code == code && p.Status == AssessmentPlanStatus.Published)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AssessmentPlan plan, CancellationToken cancellationToken = default)
    {
        await _dbContext.AssessmentPlans.AddAsync(plan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AssessmentPlan plan, CancellationToken cancellationToken = default)
    {
        _dbContext.AssessmentPlans.Update(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}