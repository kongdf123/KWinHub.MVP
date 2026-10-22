using KWinHub.Domain.Common;

namespace KWinHub.Domain.Users;

public class User : BaseEntity<Guid>
{
    public const string PlatformAdminRole = "PlatformAdmin";

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; } = UserStatus.PendingEmailVerification;

    public string? PlatformRole { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public string PreferredLanguage { get; private set; } = "en";

    public bool IsPlatformAdmin =>
        string.Equals(PlatformRole, PlatformAdminRole, StringComparison.Ordinal);

    private User()
    {
    }

    public User(
        string email,
        string passwordHash,
        string displayName,
        string preferredLanguage = "en",
        string? platformRole = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        PlatformRole = platformRole;
        Status = UserStatus.PendingEmailVerification;
    }

    public void MarkEmailVerified()
    {
        EmailVerifiedAt = DateTimeOffset.UtcNow;
        if (Status == UserStatus.PendingEmailVerification)
        {
            Status = UserStatus.Active;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void GrantPlatformAdmin()
    {
        PlatformRole = PlatformAdminRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokePlatformAdmin()
    {
        PlatformRole = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
