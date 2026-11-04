namespace KWinHub.Hub.Contracts.Organizations;

public record OrganizationMemberResponse(
    Guid UserId,
    string Role,
    string Status,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LastActiveAt);