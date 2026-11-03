using KWinHub.Platform.Users;

namespace KWinHub.Platform.Application.Registration;

public interface IVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);

    Task<EmailVerificationToken?> FindUsableByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
