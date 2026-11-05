using KWinHub.Hub.Application.Reports;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Reports;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly HubDbContext _dbContext;

    public ReportRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reports.AddAsync(report, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Report?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reports
            .AsNoTracking()
            .Include(r => r.MetricValues)
            .Include(r => r.AssetStates)
            .SingleOrDefaultAsync(
                r => r.Id == id && r.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var reports = await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        return reports.OrderByDescending(r => r.GeneratedAt).ToList();
    }

    public async Task<IReadOnlyList<Report>> ListBySubjectAsync(Guid organizationId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        var reports = await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.OrganizationId == organizationId && r.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        return reports.OrderByDescending(r => r.GeneratedAt).ToList();
    }
}