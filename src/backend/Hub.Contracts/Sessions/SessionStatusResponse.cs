namespace KWinHub.Contracts.Sessions;

public record SessionStatusResponse(
    Guid SessionId,
    string Status,
    DateTimeOffset? UpdatedAt,
    string? ErrorMessage = null);
