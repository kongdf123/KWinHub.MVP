using KWinHub.Application.Abstractions;
using KWinHub.Application.Subscriptions;
using KWinHub.Contracts.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

/// <summary>
/// Customer-facing subscription read APIs. Tenant isolation is enforced by
/// requiring the requested organization id to match the caller's organization
/// from the JWT; mismatches return 404 to avoid tenant enumeration.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1")]
public class OrganizationSubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICurrentUserContext _currentUser;

    public OrganizationSubscriptionsController(
        ISubscriptionService subscriptionService,
        ICurrentUserContext currentUser)
    {
        _subscriptionService = subscriptionService;
        _currentUser = currentUser;
    }

    [HttpGet("organizations/{organizationId:guid}/subscription")]
    [ProducesResponseType(typeof(SubscriptionSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionSummary>> GetSubscription(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var summary = await _subscriptionService.GetSummaryAsync(
            _currentUser.OrganizationId,
            organizationId,
            cancellationToken);

        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpGet("organizations/{organizationId:guid}/usage")]
    [ProducesResponseType(typeof(UsageSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsageSummary>> GetUsage(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var usage = await _subscriptionService.GetUsageAsync(
            _currentUser.OrganizationId,
            organizationId,
            cancellationToken);

        return usage is null ? NotFound() : Ok(usage);
    }

    [HttpGet("organizations/{organizationId:guid}/entitlements")]
    [ProducesResponseType(typeof(EntitlementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntitlementsResponse>> GetEntitlements(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var entitlements = await _subscriptionService.GetEntitlementsAsync(
            _currentUser.OrganizationId,
            organizationId,
            cancellationToken);

        return entitlements is null ? NotFound() : Ok(entitlements);
    }

    [HttpGet("me/access")]
    [ProducesResponseType(typeof(AccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessResponse>> GetAccess(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var orgId = organizationId ?? _currentUser.OrganizationId;
        var access = await _subscriptionService.GetAccessAsync(
            _currentUser.OrganizationId,
            orgId,
            cancellationToken);

        return access is null ? NotFound() : Ok(access);
    }
}
