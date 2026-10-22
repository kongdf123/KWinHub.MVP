using KWinHub.Domain.Common;

namespace KWinHub.Domain.Organizations;

public class Organization : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public OrganizationType Type { get; private set; } = OrganizationType.Standard;

    public OrganizationStatus Status { get; private set; } = OrganizationStatus.Active;

    public string? CountryCode { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public string? Region { get; private set; }

    public string? Timezone { get; private set; }

    public string? Locale { get; private set; }

    private Organization()
    {
    }

    public Organization(
        string name,
        string slug,
        string? region = null,
        string? timezone = null,
        string? locale = null,
        Guid? id = null,
        OrganizationType type = OrganizationType.Standard,
        string? countryCode = null,
        Guid? createdByUserId = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name;
        Slug = slug;
        Region = region;
        Timezone = timezone;
        Locale = locale;
        Type = type;
        CountryCode = countryCode;
        CreatedByUserId = createdByUserId;
    }

    public void Update(string name, string? region, string? timezone, string? locale)
    {
        Name = name;
        Region = region;
        Timezone = timezone;
        Locale = locale;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = OrganizationStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        Status = OrganizationStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
