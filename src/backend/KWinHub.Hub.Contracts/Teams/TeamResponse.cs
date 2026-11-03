namespace KWinHub.Hub.Contracts.Teams;

public record TeamResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    string? SportId,
    string? Season,
    string Status,
    int MemberCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);