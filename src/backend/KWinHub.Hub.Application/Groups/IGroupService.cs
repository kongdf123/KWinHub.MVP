using KWinHub.Hub.Contracts.Groups;

namespace KWinHub.Hub.Application.Groups;

public interface IGroupService
{
    Task<GroupResponse> CreateAsync(
        Guid organizationId,
        CreateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<GroupResponse?> GetByIdAsync(Guid organizationId, Guid groupId, CancellationToken cancellationToken = default);

    Task<List<GroupResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? groupType = null,
        CancellationToken cancellationToken = default);

    Task<GroupResponse?> UpdateAsync(
        Guid organizationId,
        Guid groupId,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid organizationId, Guid groupId, CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(Guid organizationId, Guid groupId, CancellationToken cancellationToken = default);

    Task<GroupMemberResponse?> AddMemberAsync(
        Guid organizationId,
        Guid groupId,
        AddGroupMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveMemberAsync(
        Guid organizationId,
        Guid groupId,
        Guid subjectId,
        CancellationToken cancellationToken = default);

    Task<List<GroupMemberResponse>> ListMembersAsync(
        Guid organizationId,
        Guid groupId,
        CancellationToken cancellationToken = default);
}