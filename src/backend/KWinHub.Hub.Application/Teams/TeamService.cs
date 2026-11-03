using KWinHub.Hub.Contracts.Teams;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Teams;

namespace KWinHub.Hub.Application.Teams;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;
    private readonly ISubjectRepository _subjectRepository;

    public TeamService(ITeamRepository repository, ISubjectRepository subjectRepository)
    {
        _repository = repository;
        _subjectRepository = subjectRepository;
    }

    public async Task<TeamResponse> CreateAsync(
        Guid organizationId,
        CreateTeamRequest request,
        CancellationToken cancellationToken = default)
    {
        var team = new Team(organizationId, request.Name, request.Description, request.SportId, request.Season);
        await _repository.AddAsync(team, cancellationToken);
        return await MapToResponseAsync(team, cancellationToken);
    }

    public async Task<TeamResponse?> GetByIdAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return null;
        }

        return await MapToResponseAsync(team, cancellationToken);
    }

    public async Task<List<TeamResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var teams = await _repository.ListByOrganizationAsync(organizationId, search, status, cancellationToken);
        var responses = new List<TeamResponse>(teams.Count);
        foreach (var team in teams)
        {
            responses.Add(await MapToResponseAsync(team, cancellationToken));
        }

        return responses;
    }

    public async Task<TeamResponse?> UpdateAsync(
        Guid organizationId,
        Guid teamId,
        UpdateTeamRequest request,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return null;
        }

        team.Update(request.Name, request.Description, request.SportId, request.Season);
        await _repository.UpdateAsync(team, cancellationToken);
        return await MapToResponseAsync(team, cancellationToken);
    }

    public async Task<bool> ArchiveAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return false;
        }

        team.Archive();
        await _repository.UpdateAsync(team, cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return false;
        }

        team.Restore();
        await _repository.UpdateAsync(team, cancellationToken);
        return true;
    }

    public async Task<TeamMemberResponse?> AddMemberAsync(
        Guid organizationId,
        Guid teamId,
        AddTeamMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return null;
        }

        var subject = await _subjectRepository.GetByIdAsync(organizationId, request.SubjectId, cancellationToken);
        if (subject is null || subject.OrganizationId != organizationId)
        {
            return null;
        }

        var membership = team.AddMember(request.SubjectId, request.Role);
        await _repository.AddMembershipAsync(membership, cancellationToken);

        return new TeamMemberResponse(membership.Id, team.Id, subject.Id, membership.Role);
    }

    public async Task<bool> RemoveMemberAsync(
        Guid organizationId,
        Guid teamId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return false;
        }

        var removed = await _repository.RemoveMembershipAsync(teamId, subjectId, cancellationToken);
        return removed;
    }

    public async Task<List<TeamMemberResponse>> ListMembersAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        var team = await _repository.GetByIdAsync(teamId, cancellationToken);
        if (team is null || team.OrganizationId != organizationId)
        {
            return [];
        }

        return team.Memberships
            .Select(m => new TeamMemberResponse(m.Id, team.Id, m.SubjectId, m.Role))
            .ToList();
    }

    private async Task<TeamResponse> MapToResponseAsync(Team team, CancellationToken cancellationToken)
    {
        var memberCount = await _repository.CountMembersAsync(team.Id, cancellationToken);
        return new TeamResponse(
            team.Id,
            team.OrganizationId,
            team.Name,
            team.Description,
            team.SportId,
            team.Season,
            team.Status,
            memberCount,
            team.CreatedAt,
            team.UpdatedAt);
    }
}