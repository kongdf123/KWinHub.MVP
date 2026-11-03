using KWinHub.Hub.Teams;

namespace KWinHub.Hub.Application.Teams;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<List<Team>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Team team, CancellationToken cancellationToken = default);

    Task UpdateAsync(Team team, CancellationToken cancellationToken = default);

    Task<int> CountMembersAsync(Guid teamId, CancellationToken cancellationToken = default);
}