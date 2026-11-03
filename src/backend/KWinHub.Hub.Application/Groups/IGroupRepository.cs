using KWinHub.Hub.Groups;

namespace KWinHub.Hub.Application.Groups;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<Group>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? groupType = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Group group, CancellationToken cancellationToken = default);

    Task UpdateAsync(Group group, CancellationToken cancellationToken = default);

    Task<int> CountMembersAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task AddMembershipAsync(GroupMembership membership, CancellationToken cancellationToken = default);

    Task<bool> RemoveMembershipAsync(Guid groupId, Guid subjectId, CancellationToken cancellationToken = default);
}