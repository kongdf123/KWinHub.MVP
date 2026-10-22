namespace KWinHub.Application.Abstractions;

public interface IJwtTokenIssuer
{
    string IssueToken(
        Guid userId,
        string email,
        string displayName,
        Guid organizationId,
        string organizationRole,
        bool isPlatformAdmin);
}
