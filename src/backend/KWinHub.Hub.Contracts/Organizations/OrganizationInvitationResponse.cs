namespace KWinHub.Hub.Contracts.Organizations;

public record OrganizationInvitationResponse(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string Role,
    string Status,
    DateTimeOffset ExpiresAt,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AcceptedAt,
    Guid? AcceptedBy,
    DateTimeOffset? RevokedAt,
    Guid? RevokedBy);