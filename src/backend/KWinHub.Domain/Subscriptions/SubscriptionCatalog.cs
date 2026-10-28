namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Centralized, stable catalog of feature and quota keys. Both the API
/// enforcement layer and the frontend feature registry reference these codes.
/// </summary>
public static class SubscriptionCatalog
{
    // ---- Feature keys ----
    public const string FeatureTestingSprint = "testing.sprint";
    public const string FeatureTestingForceplate = "testing.forceplate";
    public const string FeatureAnalyticsAdvanced = "analytics.advanced";
    public const string FeatureIntegrationsApi = "integrations.api";
    public const string FeatureReportsExport = "reports.export";
    public const string FeatureSubscriptionView = "subscription.view";
    public const string FeatureSubscriptionManage = "subscription.manage";

    // ---- Quota keys ----
    public const string QuotaMembersActive = "members.active";
    public const string QuotaDevicesRegistered = "devices.registered";
    public const string QuotaSessionsMonthly = "sessions.monthly";
    public const string QuotaStorageBytes = "storage.bytes";

    public static readonly IReadOnlyList<string> AllFeatureKeys = new[]
    {
        FeatureTestingSprint,
        FeatureTestingForceplate,
        FeatureAnalyticsAdvanced,
        FeatureIntegrationsApi,
        FeatureReportsExport,
        FeatureSubscriptionView,
        FeatureSubscriptionManage,
    };

    public static readonly IReadOnlyList<string> AllQuotaKeys = new[]
    {
        QuotaMembersActive,
        QuotaDevicesRegistered,
        QuotaSessionsMonthly,
        QuotaStorageBytes,
    };

    /// <summary>
    /// Unit label for each quota key, used in usage summaries.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> QuotaUnits = new Dictionary<string, string>
    {
        [QuotaMembersActive] = "members",
        [QuotaDevicesRegistered] = "devices",
        [QuotaSessionsMonthly] = "sessions",
        [QuotaStorageBytes] = "bytes",
    };
}
