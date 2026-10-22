using KWinHub.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace KWinHub.Infrastructure.Identity;

public class AspNetCorePasswordHasher : IPasswordHasher
{
    private static readonly object HasherUser = new();

    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(HasherUser, password);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(HasherUser, hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
