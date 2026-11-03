using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KWinHub.Hub.Application.CrossContext;

namespace KWinHub.Hub.Infrastructure.CrossContext;

/// <summary>
/// M0 IPlatformGateway implementation. Calls the Platform API's internal,
/// API-key protected endpoints over HTTP for feature-entitlement checks and
/// usage metering. The Hub never recomputes subscription rules (ADR-0001).
/// </summary>
public sealed class HttpPlatformGateway : IPlatformGateway
{
    public const string HttpClientName = "platform-internal";
    public const string ApiKeyHeaderName = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    public HttpPlatformGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EntitlementDecision> ResolveEntitlementAsync(
        Guid organizationId,
        string featureKey,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"internal/v1/entitlements?organizationId={organizationId}&featureKey={Uri.EscapeDataString(featureKey)}",
            cancellationToken);

        // A 404/empty subscription means the feature is not entitled; transport
        // errors propagate so OrganizationAccessService can fail closed.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new EntitlementDecision(false, "no_subscription");
        }

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<EntitlementDto>(JsonOptions, cancellationToken);
        return new EntitlementDecision(dto?.Allowed ?? false, dto?.Reason);
    }

    public async Task<long> ConsumeUsageAsync(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "internal/v1/usage/consume",
            new ConsumeUsageRequest(organizationId, metricKey, businessEventId, delta),
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var detail = await response.Content.ReadFromJsonAsync<QuotaExceededDto>(JsonOptions, cancellationToken);
            throw new QuotaExceededException(
                detail?.MetricKey ?? metricKey,
                detail?.Consumed ?? 0,
                detail?.Limit,
                detail?.Requested ?? delta,
                detail?.Message);
        }

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<ConsumeUsageResponse>(JsonOptions, cancellationToken);
        return dto?.Balance ?? delta;
    }

    public async Task<bool> ValidateSecurityStampAsync(
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"internal/v1/users/{userId}/security-stamp?stamp={Uri.EscapeDataString(securityStamp)}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<SecurityStampDto>(JsonOptions, cancellationToken);
        return dto?.Valid ?? false;
    }

    private sealed record EntitlementDto(bool Allowed, string? Reason);

    private sealed record ConsumeUsageRequest(
        Guid OrganizationId,
        string MetricKey,
        string BusinessEventId,
        long Delta);

    private sealed record ConsumeUsageResponse(long Balance);

    private sealed record SecurityStampDto(bool Valid);

    private sealed record QuotaExceededDto(
        string? MetricKey,
        long Consumed,
        long? Limit,
        long Requested,
        string? Message);
}