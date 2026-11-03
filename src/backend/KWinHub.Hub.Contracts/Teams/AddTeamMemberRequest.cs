namespace KWinHub.Hub.Contracts.Teams;

public record AddTeamMemberRequest(
    Guid SubjectId,
    string? Role = null);