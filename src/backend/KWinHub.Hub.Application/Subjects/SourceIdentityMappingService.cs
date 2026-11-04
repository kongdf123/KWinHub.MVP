using KWinHub.Hub.Contracts.Subjects;
using KWinHub.Hub.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public class SourceIdentityMappingService : ISourceIdentityMappingService
{
    private readonly ISourceIdentityMappingRepository _repository;

    public SourceIdentityMappingService(ISourceIdentityMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<SourceIdentityMappingResponse> RegisterAsync(
        Guid organizationId,
        CreateSourceIdentityMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetBySourceIdentityAsync(
            organizationId,
            request.SourceApp,
            request.InstallationScope,
            request.ExternalId,
            cancellationToken);

        if (existing is null)
        {
            var mapping = new SourceIdentityMapping(
                organizationId: organizationId,
                sourceApp: request.SourceApp,
                installationScope: request.InstallationScope,
                externalId: request.ExternalId,
                subjectId: request.SubjectId);

            await _repository.AddAsync(mapping, cancellationToken);
            return ToResponse(mapping);
        }

        if (existing.SubjectId == request.SubjectId)
        {
            return ToResponse(existing);
        }

        existing.MarkConflict(
            $"Source identity maps to subject {existing.SubjectId} but registration requested {request.SubjectId}.");
        await _repository.UpdateAsync(existing, cancellationToken);

        return ToResponse(existing);
    }

    public async Task<SourceIdentityMappingResponse?> ResolveConflictAsync(
        Guid organizationId,
        Guid mappingId,
        ResolveSourceIdentityConflictRequest request,
        Guid resolvedBy,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _repository.ListByOrganizationAsync(
            organizationId,
            cancellationToken: cancellationToken);

        var mapping = mappings.FirstOrDefault(m => m.Id == mappingId);
        if (mapping is null || !mapping.IsConflict)
        {
            return null;
        }

        mapping.Resolve(request.SubjectId, resolvedBy);
        await _repository.UpdateAsync(mapping, cancellationToken);

        return ToResponse(mapping);
    }

    public async Task<IReadOnlyList<SourceIdentityMappingResponse>> ListAsync(
        Guid organizationId,
        string? status = null,
        string? sourceApp = null,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _repository.ListByOrganizationAsync(
            organizationId,
            status,
            sourceApp,
            cancellationToken);

        return mappings.Select(ToResponse).ToList();
    }

    public async Task<SourceIdentityMappingResponse?> GetBySourceIdentityAsync(
        Guid organizationId,
        string sourceApp,
        string installationScope,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var mapping = await _repository.GetBySourceIdentityAsync(
            organizationId,
            sourceApp,
            installationScope,
            externalId,
            cancellationToken);

        return mapping is null ? null : ToResponse(mapping);
    }

    public async Task<IReadOnlyList<SourceIdentityMappingResponse>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _repository.ListBySubjectAsync(
            organizationId,
            subjectId,
            cancellationToken);

        return mappings.Select(ToResponse).ToList();
    }

    private static SourceIdentityMappingResponse ToResponse(SourceIdentityMapping mapping) =>
        new(
            Id: mapping.Id,
            OrganizationId: mapping.OrganizationId,
            SourceApp: mapping.SourceApp,
            InstallationScope: mapping.InstallationScope,
            ExternalId: mapping.ExternalId,
            SubjectId: mapping.SubjectId,
            Status: mapping.Status,
            ConflictReason: mapping.ConflictReason,
            ResolvedBy: mapping.ResolvedBy,
            ResolvedAt: mapping.ResolvedAt,
            CreatedAt: mapping.CreatedAt,
            UpdatedAt: mapping.UpdatedAt);
}