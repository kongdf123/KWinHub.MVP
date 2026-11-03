using KWinHub.Application.Abstractions;
using KWinHub.Application.Registration;
using KWinHub.Domain.Users;

namespace KWinHub.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private const string PlatformOrganizationName = "KWinHub Platform";
    private const string NoOrganizationName = "No organization";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await _users.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        if (user.Status is UserStatus.Suspended or UserStatus.Disabled)
        {
            return null;
        }

        var organization = await _users.GetPrimaryOrganizationAsync(user.Id, cancellationToken);
        var authenticatedUser = BuildAuthenticatedUser(user, organization);

        user.RecordLogin();
        await _users.UpdateAsync(user, cancellationToken);

        return authenticatedUser;
    }

    public async Task<AuthenticatedUser?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.Status is UserStatus.Suspended or UserStatus.Disabled)
        {
            return null;
        }

        var organization = await _users.GetPrimaryOrganizationAsync(user.Id, cancellationToken);
        return BuildAuthenticatedUser(user, organization);
    }

    private static AuthenticatedUser BuildAuthenticatedUser(
        User user,
        (Guid OrganizationId, string OrganizationName, string Role)? organization)
    {
        var organizationId = organization?.OrganizationId ?? Guid.Empty;
        var organizationName = organization?.OrganizationName
            ?? (user.IsPlatformAdmin ? PlatformOrganizationName : NoOrganizationName);
        var organizationRole = organization?.Role
            ?? (user.IsPlatformAdmin ? User.PlatformAdminRole : "Member");

        return new AuthenticatedUser(
            UserId: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            OrganizationId: organizationId,
            OrganizationName: organizationName,
            OrganizationRole: organizationRole,
            IsPlatformAdmin: user.IsPlatformAdmin,
            EmailVerified: user.EmailVerifiedAt is not null,
            Status: user.Status.ToString());
    }
}
