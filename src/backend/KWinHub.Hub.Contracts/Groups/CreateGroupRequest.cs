namespace KWinHub.Hub.Contracts.Groups;

public record CreateGroupRequest(
    string Name,
    string? Description = null,
    string? GroupType = null);