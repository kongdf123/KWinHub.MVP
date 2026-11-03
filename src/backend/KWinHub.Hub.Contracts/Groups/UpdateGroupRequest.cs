namespace KWinHub.Hub.Contracts.Groups;

public record UpdateGroupRequest(
    string Name,
    string? Description = null,
    string? GroupType = null);