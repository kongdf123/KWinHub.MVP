using KWinHub.Hub.Plans;

namespace KWinHub.Hub.Application.Plans;

public interface IAssessmentPlanRepository
{
    Task<AssessmentPlan?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssessmentPlan>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssessmentPlan>> ListByCodeAsync(Guid organizationId, string code, CancellationToken cancellationToken = default);
    Task<AssessmentPlan?> GetPublishedByCodeAsync(Guid organizationId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(AssessmentPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(AssessmentPlan plan, CancellationToken cancellationToken = default);
}