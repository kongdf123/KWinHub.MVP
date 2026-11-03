namespace KWinHub.Platform.Application.Authentication;

public record AuthenticatedUser(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationRole,
    bool IsPlatformAdmin,
    bool EmailVerified,
    string Status,
    string SecurityStamp);

public interface IAuthenticationService
{
    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}