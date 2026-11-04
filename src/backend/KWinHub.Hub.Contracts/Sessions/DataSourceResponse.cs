namespace KWinHub.Hub.Contracts.Sessions;

public record DataSourceResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Code,
    string SourceApp,
    string? Version,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);