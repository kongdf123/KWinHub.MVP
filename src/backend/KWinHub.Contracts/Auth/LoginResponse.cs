namespace KWinHub.Contracts.Auth;

public record LoginResponse(
    string Token,
    string Type,
    Guid UserId,
    string Email,
    string DisplayName,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationRole,
    bool IsPlatformAdmin,
    bool EmailVerified,
    string Status);
