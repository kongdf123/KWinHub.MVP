using KWinHub.Hub.Contracts.Subjects;
using KWinHub.Hub.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repository;

    public SubjectService(ISubjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubjectResponse> CreateAsync(
        Guid organizationId,
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SubjectType>(request.SubjectType, true, out var subjectType))
        {
            throw new ArgumentException($"Invalid subject type: {request.SubjectType}", nameof(request));
        }

        var subject = new Subject(
            organizationId: organizationId,
            firstName: request.FirstName,
            lastName: request.LastName,
            subjectType: subjectType,
            externalId: request.ExternalId,
            displayName: request.DisplayName,
            dateOfBirth: request.DateOfBirth,
            sex: request.Sex);

        await _repository.AddAsync(subject, cancellationToken);

        return ToResponse(subject);
    }

    public async Task<SubjectResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var subject = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (subject is null)
        {
            return null;
        }

        if (!Enum.TryParse<SubjectType>(request.SubjectType, true, out var subjectType))
        {
            throw new ArgumentException($"Invalid subject type: {request.SubjectType}", nameof(request));
        }

        subject.Update(
            firstName: request.FirstName,
            lastName: request.LastName,
            subjectType: subjectType,
            externalId: request.ExternalId,
            displayName: request.DisplayName,
            dateOfBirth: request.DateOfBirth,
            sex: request.Sex);

        await _repository.UpdateAsync(subject, cancellationToken);

        return ToResponse(subject);
    }

    public async Task<IReadOnlyList<SubjectResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? subjectType = null,
        CancellationToken cancellationToken = default)
    {
        var subjects = await _repository.ListByOrganizationAsync(
            organizationId,
            search,
            status,
            subjectType,
            cancellationToken);
        return subjects.Select(ToResponse).ToList();
    }

    public async Task<SubjectResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subject = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return subject is null ? null : ToResponse(subject);
    }

    public async Task<bool> ArchiveAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subject = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (subject is null)
        {
            return false;
        }

        subject.Archive();
        await _repository.UpdateAsync(subject, cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var subject = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (subject is null)
        {
            return false;
        }

        subject.Restore();
        await _repository.UpdateAsync(subject, cancellationToken);
        return true;
    }

    private static SubjectResponse ToResponse(Subject subject) =>
        new(
            Id: subject.Id,
            OrganizationId: subject.OrganizationId,
            FirstName: subject.FirstName,
            LastName: subject.LastName,
            SubjectType: subject.SubjectType.ToString(),
            ExternalId: subject.ExternalId,
            DisplayName: subject.DisplayName,
            DateOfBirth: subject.DateOfBirth,
            Sex: subject.Sex,
            Status: subject.Status,
            CreatedAt: subject.CreatedAt,
            UpdatedAt: subject.UpdatedAt);
}