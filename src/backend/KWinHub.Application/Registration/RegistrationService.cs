using System.Security.Cryptography;
using System.Text;
using KWinHub.Application.Abstractions;
using KWinHub.Contracts.Auth;
using KWinHub.Contracts.Registrations;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Users;

namespace KWinHub.Application.Registration;

public class RegistrationService : IRegistrationService
{
    private static readonly TimeSpan VerificationLifetime = TimeSpan.FromHours(24);
    private const string StandardAccountType = "Standard";
    private const string IndividualAccountType = "Individual";

    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _organizations;
    private readonly IVerificationTokenRepository _tokens;
    private readonly IPasswordHasher _passwordHasher;

    public RegistrationService(
        IUserRepository users,
        IOrganizationRepository organizations,
        IVerificationTokenRepository tokens,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _organizations = organizations;
        _tokens = tokens;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegistrationResponse> RegisterAsync(
        RegisterRequest request,
        bool exposeVerificationToken,
        CancellationToken cancellationToken = default)
    {
        var accountType = (request.AccountType ?? string.Empty).Trim();
        if (!IsAccountType(accountType, StandardAccountType) && !IsAccountType(accountType, IndividualAccountType))
        {
            throw new ArgumentException("Account type must be 'Standard' or 'Individual'.", nameof(request));
        }

        if (!request.AcceptTerms)
        {
            throw new ArgumentException("You must accept the terms of service to register.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
        {
            throw new ArgumentException("A valid email address is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.Organization?.Type)
            && !IsAccountType(request.Organization!.Type, StandardAccountType)
            && !IsAccountType(request.Organization!.Type, IndividualAccountType))
        {
            throw new ArgumentException("Organization type must be 'Standard' or 'Individual'.", nameof(request));
        }

        var email = NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var isIndividual =
            IsAccountType(accountType, IndividualAccountType)
            || IsAccountType(request.Organization?.Type, IndividualAccountType);
        var organizationType = isIndividual ? OrganizationType.Individual : OrganizationType.Standard;

        var organizationName = ResolveOrganizationName(request, isIndividual);
        var slug = await ResolveUniqueSlugAsync(organizationName, cancellationToken);

        var user = new User(
            email: email,
            passwordHash: _passwordHasher.Hash(request.Password),
            displayName: request.DisplayName.Trim(),
            preferredLanguage: string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage,
            platformRole: null);

        var organization = new Organization(
            name: organizationName,
            slug: slug,
            locale: string.IsNullOrWhiteSpace(request.PreferredLanguage) ? null : request.PreferredLanguage,
            id: Guid.NewGuid(),
            type: organizationType,
            countryCode: request.Organization?.CountryCode ?? request.CountryCode,
            createdByUserId: user.Id);

        var membership = new OrganizationMembership(
            userId: user.Id,
            organizationId: organization.Id,
            role: "OrganizationAdmin");

        var rawToken = GenerateRawToken();
        var token = new EmailVerificationToken(
            userId: user.Id,
            tokenHash: HashToken(rawToken),
            lifetime: VerificationLifetime);

        await _users.AddAsync(user, cancellationToken);
        await _organizations.AddAsync(organization, cancellationToken);
        await _organizations.AddMembershipAsync(membership, cancellationToken);
        await _tokens.AddAsync(token, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return new RegistrationResponse(
            UserId: user.Id,
            Email: user.Email,
            OrganizationId: organization.Id,
            OrganizationName: organization.Name,
            OrganizationType: organization.Type.ToString(),
            UserStatus: user.Status.ToString(),
            VerificationToken: exposeVerificationToken ? rawToken : null,
            VerificationExpiresAt: token.ExpiresAt);
    }

    public async Task<VerifyEmailResponse?> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var verificationToken = await _tokens.FindUsableByHashAsync(HashToken(token), cancellationToken);
        if (verificationToken is null)
        {
            return null;
        }

        var user = await _users.GetByIdAsync(verificationToken.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        verificationToken.Consume();
        user.MarkEmailVerified();
        await _users.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(true, user.Email, user.Status.ToString());
    }

    public async Task<bool> ResendVerificationAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var user = await _users.GetByEmailAsync(NormalizeEmail(email), cancellationToken);
        if (user is null || user.EmailVerifiedAt is not null)
        {
            return false;
        }

        await _tokens.InvalidateAllForUserAsync(user.Id, cancellationToken);

        var token = new EmailVerificationToken(
            userId: user.Id,
            tokenHash: HashToken(GenerateRawToken()),
            lifetime: VerificationLifetime);

        await _tokens.AddAsync(token, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string ResolveOrganizationName(RegisterRequest request, bool isIndividual)
    {
        if (isIndividual)
        {
            if (!string.IsNullOrWhiteSpace(request.PersonalWorkspaceName))
            {
                return request.PersonalWorkspaceName.Trim();
            }

            return $"{request.DisplayName.Trim()} Workspace";
        }

        var name = request.Organization?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Organization name is required for a standard account.", nameof(request));
        }

        return name;
    }

    private async Task<string> ResolveUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = BuildSlug(name);
        var slug = baseSlug;

        for (var attempt = 0; await _organizations.SlugExistsAsync(slug, cancellationToken); attempt++)
        {
            if (attempt >= 10)
            {
                throw new InvalidOperationException("Unable to allocate a unique organization slug.");
            }

            var suffix = GenerateSuffix(6);
            var maxBaseLength = 100 - suffix.Length - 1;
            var trimmed = baseSlug.Length > maxBaseLength
                ? baseSlug[..maxBaseLength].Trim('-')
                : baseSlug;
            slug = $"{trimmed}-{suffix}";
        }

        return slug;
    }

    private static string BuildSlug(string name)
    {
        var builder = new StringBuilder();
        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if ((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
            {
                builder.Append(character);
            }
            else if ((character == ' ' || character == '-' || character == '_' || character == '/')
                     && builder.Length > 0
                     && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length == 0)
        {
            slug = "workspace";
        }

        if (slug.Length > 100)
        {
            slug = slug[..100].Trim('-');
        }

        return slug;
    }

    private static string GenerateSuffix(int length)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        var result = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            result.Append(alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]);
        }

        return result.ToString();
    }

    private static string GenerateRawToken() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsAccountType(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    private static bool IsValidEmail(string email)
    {
        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex != trimmed.LastIndexOf('@') || atIndex == trimmed.Length - 1)
        {
            return false;
        }

        var domain = trimmed[(atIndex + 1)..];
        return domain.Contains('.')
               && !domain.StartsWith('.')
               && !domain.EndsWith('.')
               && !trimmed.Contains(' ');
    }
}
