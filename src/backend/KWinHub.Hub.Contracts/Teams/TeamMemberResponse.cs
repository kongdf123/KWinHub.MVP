namespace KWinHub.Hub.Contracts.Teams;

public record TeamMemberResponse(
    Guid MembershipId,
    Guid TeamId,
    Guid SubjectId,
    string? Role);