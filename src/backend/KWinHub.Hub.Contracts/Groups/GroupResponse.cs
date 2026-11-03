namespace KWinHub.Hub.Contracts.Groups;

public record GroupResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    string? GroupType,
    string Status,
    int MemberCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);