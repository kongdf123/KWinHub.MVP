using KWinHub.Hub.Contracts.Teams;

namespace KWinHub.Hub.Application.Teams;

public interface ITeamService
{
    Task<TeamResponse> CreateAsync(
        Guid organizationId,
        CreateTeamRequest request,
        CancellationToken cancellationToken = default);

    Task<TeamResponse?> GetByIdAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<List<TeamResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<TeamResponse?> UpdateAsync(
        Guid organizationId,
        Guid teamId,
        UpdateTeamRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<TeamMemberResponse?> AddMemberAsync(
        Guid organizationId,
        Guid teamId,
        AddTeamMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveMemberAsync(
        Guid organizationId,
        Guid teamId,
        Guid subjectId,
        CancellationToken cancellationToken = default);

    Task<List<TeamMemberResponse>> ListMembersAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default);
}