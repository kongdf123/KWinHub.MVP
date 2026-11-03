namespace KWinHub.Contracts.Registrations;

public record RegistrationResponse(
    Guid UserId,
    string Email,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationType,
    string UserStatus,
    string? VerificationToken,
    DateTimeOffset? VerificationExpiresAt);
