using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Contracts.Groups;
using KWinHub.Hub.Groups;

namespace KWinHub.Hub.Application.Groups;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _repository;
    private readonly ISubjectRepository _subjectRepository;

    public GroupService(IGroupRepository repository, ISubjectRepository subjectRepository)
    {
        _repository = repository;
        _subjectRepository = subjectRepository;
    }

    public async Task<GroupResponse> CreateAsync(
        Guid organizationId,
        CreateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = new Group(organizationId, request.Name, request.Description, request.GroupType);
        await _repository.AddAsync(group, cancellationToken);
        return await MapToResponseAsync(group, cancellationToken);
    }

    public async Task<GroupResponse?> GetByIdAsync(
        Guid organizationId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return null;
        }

        return await MapToResponseAsync(group, cancellationToken);
    }

    public async Task<List<GroupResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? groupType = null,
        CancellationToken cancellationToken = default)
    {
        var groups = await _repository.ListByOrganizationAsync(organizationId, search, status, groupType, cancellationToken);
        var responses = new List<GroupResponse>(groups.Count);
        foreach (var group in groups)
        {
            responses.Add(await MapToResponseAsync(group, cancellationToken));
        }

        return responses;
    }

    public async Task<GroupResponse?> UpdateAsync(
        Guid organizationId,
        Guid groupId,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return null;
        }

        group.Update(request.Name, request.Description, request.GroupType);
        await _repository.UpdateAsync(group, cancellationToken);
        return await MapToResponseAsync(group, cancellationToken);
    }

    public async Task<bool> ArchiveAsync(
        Guid organizationId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return false;
        }

        group.Archive();
        await _repository.UpdateAsync(group, cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid organizationId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return false;
        }

        group.Restore();
        await _repository.UpdateAsync(group, cancellationToken);
        return true;
    }

    public async Task<GroupMemberResponse?> AddMemberAsync(
        Guid organizationId,
        Guid groupId,
        AddGroupMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return null;
        }

        var subject = await _subjectRepository.GetByIdAsync(organizationId, request.SubjectId, cancellationToken);
        if (subject is null || subject.OrganizationId != organizationId)
        {
            return null;
        }

        var membership = group.AddMember(request.SubjectId, request.Role);
        await _repository.AddMembershipAsync(membership, cancellationToken);

        return new GroupMemberResponse(membership.Id, group.Id, subject.Id, membership.Role);
    }

    public async Task<bool> RemoveMemberAsync(
        Guid organizationId,
        Guid groupId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return false;
        }

        var removed = await _repository.RemoveMembershipAsync(groupId, subjectId, cancellationToken);
        return removed;
    }

    public async Task<List<GroupMemberResponse>> ListMembersAsync(
        Guid organizationId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(groupId, cancellationToken);
        if (group is null || group.OrganizationId != organizationId)
        {
            return [];
        }

        return group.Memberships
            .Select(m => new GroupMemberResponse(m.Id, group.Id, m.SubjectId, m.Role))
            .ToList();
    }

    private async Task<GroupResponse> MapToResponseAsync(Group group, CancellationToken cancellationToken)
    {
        var memberCount = await _repository.CountMembersAsync(group.Id, cancellationToken);
        return new GroupResponse(
            group.Id,
            group.OrganizationId,
            group.Name,
            group.Description,
            group.GroupType,
            group.Status,
            memberCount,
            group.CreatedAt,
            group.UpdatedAt);
    }
}