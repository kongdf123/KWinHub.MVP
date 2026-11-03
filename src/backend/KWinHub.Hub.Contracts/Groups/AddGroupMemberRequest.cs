namespace KWinHub.Hub.Contracts.Groups;

public record AddGroupMemberRequest(
    Guid SubjectId,
    string? Role = null);