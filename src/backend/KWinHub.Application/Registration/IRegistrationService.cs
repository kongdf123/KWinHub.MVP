using KWinHub.Contracts.Auth;
using KWinHub.Contracts.Registrations;

namespace KWinHub.Application.Registration;

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
