namespace KWinHub.Contracts.Protocols;

public record ProtocolResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Code,
    string Version,
    string? Category,
    string? Sport,
    string Status,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
