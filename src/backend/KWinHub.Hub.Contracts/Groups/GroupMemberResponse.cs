namespace KWinHub.Hub.Contracts.Groups;

public record GroupMemberResponse(
    Guid MembershipId,
    Guid GroupId,
    Guid SubjectId,
    string? Role);