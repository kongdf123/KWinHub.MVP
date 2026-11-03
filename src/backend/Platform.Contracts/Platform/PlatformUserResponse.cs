namespace KWinHub.Contracts.Platform;

public record PlatformUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    string? PlatformRole,
    bool EmailVerified,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    int OrganizationCount);

public record UpdateOrganizationStatusRequest(string Action);

public record UpdateUserStatusRequest(string Action);
