using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KWinHub.Platform.Application.CrossContext;

namespace KWinHub.Platform.Infrastructure.CrossContext;

/// <summary>
/// M0 IHubGateway implementation. Calls the Hub API's internal, API-key
/// protected endpoints over HTTP so the two bounded contexts remain independently
/// deployable with no shared database access. M1-2 replaces the membership
/// provisioning write with an outbox/event relay; the read calls stay as-is.
/// </summary>
public sealed class HttpHubGateway : IHubGateway
{
    public const string HttpClientName = "hub-internal";
    public const string ApiKeyHeaderName = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    public HttpHubGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ProvisionAdminMembershipAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "internal/v1/memberships",
            new ProvisionMembershipRequest(organizationId, userId, role),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<HubPrimaryMembership?> GetPrimaryMembershipAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"internal/v1/users/{userId}/primary-membership",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PrimaryMembershipDto>(JsonOptions, cancellationToken);
        return dto is null ? null : new HubPrimaryMembership(dto.OrganizationId, dto.Role);
    }

    public async Task<IReadOnlyList<HubMembership>> ListMembershipsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var dtos = await _httpClient.GetFromJsonAsync<List<MembershipDto>>(
            $"internal/v1/organizations/{organizationId}/memberships",
            JsonOptions,
            cancellationToken);

        return (dtos ?? [])
            .Select(d => new HubMembership(d.UserId, d.Role, d.Status, d.JoinedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<HubOrganizationStats>> GetOrganizationStatsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken = default)
    {
        var dtos = await PostBatchAsync<OrganizationStatsResponse>(
            "internal/v1/workspace-stats/organizations",
            new IdBatchRequest(organizationIds),
            cancellationToken);

        return (dtos ?? [])
            .Select(d => new HubOrganizationStats(
                d.OrganizationId,
                d.MemberCount,
                d.SubjectCount,
                d.SessionCount,
                d.ResultCount))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetUserMembershipCountsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var dtos = await PostBatchAsync<UserCountResponse>(
            "internal/v1/workspace-stats/users",
            new IdBatchRequest(userIds),
            cancellationToken);

        return (dtos ?? []).ToDictionary(d => d.UserId, d => d.MembershipCount);
    }

    public async Task PublishOrganizationLifecycleEventAsync(
        Guid eventId,
        Guid organizationId,
        string eventType,
        long eventVersion,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new LifecycleEventRequest(eventId, organizationId, eventType, eventVersion, payloadJson),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private async Task<List<T>?> PostBatchAsync<T>(
        string requestUri,
        IdBatchRequest payload,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, payload, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
    }

    private sealed record ProvisionMembershipRequest(Guid OrganizationId, Guid UserId, string Role);

    private sealed record PrimaryMembershipDto(Guid OrganizationId, string Role);

    private sealed record MembershipDto(Guid UserId, string Role, string Status, DateTimeOffset JoinedAt);

    private sealed record IdBatchRequest(IReadOnlyCollection<Guid> Ids);

    private sealed record OrganizationStatsResponse(
        Guid OrganizationId,
        int MemberCount,
        int SubjectCount,
        int SessionCount,
        int ResultCount);

    private sealed record UserCountResponse(Guid UserId, int MembershipCount);

    private sealed record LifecycleEventRequest(
        Guid EventId,
        Guid OrganizationId,
        string EventType,
        long EventVersion,
        string PayloadJson);
}