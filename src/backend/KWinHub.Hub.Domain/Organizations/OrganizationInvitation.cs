using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Organizations;

public class OrganizationInvitation : BaseEntity<Guid>
{
    public const string StatusPending = "Pending";
    public const string StatusAccepted = "Accepted";
    public const string StatusRevoked = "Revoked";
    public const string StatusExpired = "Expired";

    public Guid OrganizationId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string Role { get; private set; } = OrganizationRoles.Member;

    public string Token { get; private set; } = string.Empty;

    public string Status { get; private set; } = StatusPending;

    public DateTimeOffset ExpiresAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public Guid? AcceptedBy { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? RevokedBy { get; private set; }

    private OrganizationInvitation()
    {
    }

    public OrganizationInvitation(
        Guid organizationId,
        string email,
        string role,
        Guid createdBy,
        TimeSpan validity)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Email = email.Trim().ToLowerInvariant();
        Role = OrganizationRoles.Normalize(role);
        Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        Status = StatusPending;
        ExpiresAt = DateTimeOffset.UtcNow.Add(validity);
        CreatedBy = createdBy;
    }

    public bool IsPending => Status == StatusPending;

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    public bool IsRevoked => Status == StatusRevoked;

    public bool IsAccepted => Status == StatusAccepted;

    public bool CanAccept => IsPending && !IsExpired && !IsRevoked;

    public void Accept(Guid acceptedBy)
    {
        if (IsAccepted)
        {
            return;
        }

        if (IsRevoked)
        {
            throw new InvalidOperationException("Invitation has been revoked.");
        }

        if (IsExpired)
        {
            Status = StatusExpired;
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = StatusAccepted;
        AcceptedBy = acceptedBy;
        AcceptedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(Guid revokedBy)
    {
        if (IsAccepted)
        {
            throw new InvalidOperationException("Cannot revoke an accepted invitation.");
        }

        Status = StatusRevoked;
        RevokedBy = revokedBy;
        RevokedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}