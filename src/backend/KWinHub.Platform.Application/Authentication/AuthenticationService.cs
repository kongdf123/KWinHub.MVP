using KWinHub.Platform.Application.Abstractions;
using KWinHub.Platform.Application.Audit;
using KWinHub.Platform.Application.CrossContext;
using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Users;

namespace KWinHub.Platform.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private const string PlatformOrganizationName = "KWinHub Platform";
    private const string NoOrganizationName = "No organization";

    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _organizations;
    private readonly IHubGateway _hubGateway;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;

    public AuthenticationService(
        IUserRepository users,
        IOrganizationRepository organizations,
        IHubGateway hubGateway,
        IPasswordHasher passwordHasher,
        IAuditService audit)
    {
        _users = users;
        _organizations = organizations;
        _hubGateway = hubGateway;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        string? ipAddress = null,
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

        var organization = await ResolveOrganizationAsync(user.Id, cancellationToken);
        var authenticatedUser = BuildAuthenticatedUser(user, organization);

        user.RecordLogin();
        await _users.UpdateAsync(user, cancellationToken);

        await _audit.RecordAsync(
            "user.login",
            "User",
            user.Id.ToString(),
            $"User logged in: {user.Email}",
            actorUserId: user.Id,
            actorEmail: user.Email,
            ipAddress: ipAddress,
            cancellationToken: cancellationToken);

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

        var organization = await ResolveOrganizationAsync(user.Id, cancellationToken);
        return BuildAuthenticatedUser(user, organization);
    }

    private async Task<(Guid OrganizationId, string OrganizationName, string Role)?> ResolveOrganizationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Membership lives in the Hub context; the org name is Platform-owned.
        var membership = await _hubGateway.GetPrimaryMembershipAsync(userId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        var organization = await _organizations.GetByIdAsync(membership.OrganizationId, cancellationToken);
        return (membership.OrganizationId, organization?.Name ?? string.Empty, membership.Role);
    }

    private static AuthenticatedUser BuildAuthenticatedUser(
        User user,
        (Guid OrganizationId, string OrganizationName, string Role)? organization)
    {
        var organizationId = organization?.OrganizationId ?? Guid.Empty;
        var organizationName = organization?.OrganizationName;
        if (string.IsNullOrWhiteSpace(organizationName))
        {
            organizationName = user.IsPlatformAdmin ? PlatformOrganizationName : NoOrganizationName;
        }

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
            Status: user.Status.ToString(),
            SecurityStamp: user.SecurityStamp);
    }
}