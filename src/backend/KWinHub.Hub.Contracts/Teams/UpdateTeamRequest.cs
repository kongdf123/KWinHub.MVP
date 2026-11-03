namespace KWinHub.Hub.Contracts.Teams;

public record UpdateTeamRequest(
    string Name,
    string? Description = null,
    string? SportId = null,
    string? Season = null);