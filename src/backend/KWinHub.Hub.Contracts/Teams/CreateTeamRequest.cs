namespace KWinHub.Hub.Contracts.Teams;

public record CreateTeamRequest(
    string Name,
    string? Description = null,
    string? SportId = null,
    string? Season = null);