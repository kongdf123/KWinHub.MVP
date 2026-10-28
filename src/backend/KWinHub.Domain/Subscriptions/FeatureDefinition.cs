using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Stable, centrally-defined feature key. Created via controlled migration/seeding;
/// UI views map to these same canonical codes for enforcement.
/// </summary>
public class FeatureDefinition : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Default policy when no plan/override specifies the feature:
    /// true = enabled by default, false = disabled by default.
    /// </summary>
    public bool DefaultEnabled { get; private set; }

    private FeatureDefinition()
    {
    }

    public FeatureDefinition(string key, string category, string displayName, bool defaultEnabled = false)
    {
        Id = Guid.NewGuid();
        Key = key;
        Category = category;
        DisplayName = displayName;
        DefaultEnabled = defaultEnabled;
    }
}
