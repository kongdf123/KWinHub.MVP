using KWinHub.Platform.Contracts.Auth;
using KWinHub.Platform.Contracts.Registrations;

namespace KWinHub.Platform.Application.Registration;

public interface IRegistrationService
{
    Task<RegistrationResponse> RegisterAsync(
        RegisterRequest request,
        bool exposeVerificationToken,
        CancellationToken cancellationToken = default);

    Task<VerifyEmailResponse?> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> ResendVerificationAsync(
        string email,
        CancellationToken cancellationToken = default);
}
