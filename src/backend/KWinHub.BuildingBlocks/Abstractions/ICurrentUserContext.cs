namespace KWinHub.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid UserId { get; }

    Guid OrganizationId { get; }

    string? OrganizationRole { get; }

    bool IsAuthenticated { get; }
}