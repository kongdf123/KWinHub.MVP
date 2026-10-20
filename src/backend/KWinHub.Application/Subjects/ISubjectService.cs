using KWinHub.Contracts.Subjects;

namespace KWinHub.Application.Subjects;

public interface ISubjectService
{
    Task<SubjectResponse> CreateAsync(
        Guid organizationId,
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubjectResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<SubjectResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}