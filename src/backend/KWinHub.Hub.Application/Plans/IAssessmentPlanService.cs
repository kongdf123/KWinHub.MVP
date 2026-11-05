using KWinHub.Hub.Contracts.Plans;

namespace KWinHub.Hub.Application.Plans;

public interface IAssessmentPlanService
{
    Task<AssessmentPlanResponse> CreateAsync(Guid organizationId, Guid userId, CreateAssessmentPlanRequest request, CancellationToken cancellationToken = default);
    Task<AssessmentPlanResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssessmentPlanResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<AssessmentPlanResponse?> UpdateAsync(Guid organizationId, Guid id, UpdateAssessmentPlanRequest request, CancellationToken cancellationToken = default);
    Task<AssessmentPlanResponse?> PublishAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<AssessmentPlanResponse?> RetireAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<AssessmentPlanResponse?> CreateNewVersionAsync(Guid organizationId, Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<PlanCompletenessResponse> CheckCompletenessAsync(Guid organizationId, Guid planId, Guid subjectId, IReadOnlyList<Guid> selectedSessionIds, CancellationToken cancellationToken = default);
}