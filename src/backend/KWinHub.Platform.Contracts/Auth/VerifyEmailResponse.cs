namespace KWinHub.Platform.Contracts.Auth;

public record VerifyEmailResponse(bool Verified, string Email, string Status);
