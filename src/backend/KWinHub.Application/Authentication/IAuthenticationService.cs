namespace KWinHub.Application.Authentication;

public record AuthenticatedUser(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationRole,
    bool IsPlatformAdmin,
    bool EmailVerified,
    string Status);

public interface IAuthenticationService
{
    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
