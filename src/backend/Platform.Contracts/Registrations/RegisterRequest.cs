namespace KWinHub.Contracts.Registrations;

public record RegisterOrganizationDetails(
    string Name,
    string Type,
    string CountryCode,
    string? Website = null,
    string? IntendedUse = null);

public record RegisterRequest(
    string AccountType,
    string Email,
    string Password,
    string DisplayName,
    RegisterOrganizationDetails? Organization = null,
    string? PersonalWorkspaceName = null,
    string? PrimaryRole = null,
    string? CountryCode = null,
    bool AcceptTerms = false,
    string TermsVersion = "2026-10",
    string PreferredLanguage = "en",
    bool MarketingConsent = false);
