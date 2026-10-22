using KWinHub.Domain.Common;

namespace KWinHub.Domain.Users;

public class EmailVerificationToken : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public bool IsUsable => ConsumedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    private EmailVerificationToken()
    {
    }

    public EmailVerificationToken(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime);
    }

    public void Consume()
    {
        ConsumedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
