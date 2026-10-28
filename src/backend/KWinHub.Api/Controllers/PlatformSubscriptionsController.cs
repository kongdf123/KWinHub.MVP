using KWinHub.Application.Abstractions;
using KWinHub.Application.Subscriptions;
using KWinHub.Contracts.Subscriptions;
using KWinHub.Domain.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

/// <summary>
/// Platform-facing subscription management. Requires the PlatformAdmin policy.
/// All mutations require a mandatory reason and produce an audit event.
/// Subscription mutations use optimistic concurrency via the If-Match header
/// (expected revision); a stale revision returns 409.
/// </summary>
[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform")]
public class PlatformSubscriptionsController : ControllerBase
{
    private readonly IPlatformSubscriptionService _platformService;
    private readonly ICurrentUserContext _currentUser;

    public PlatformSubscriptionsController(
        IPlatformSubscriptionService platformService,
        ICurrentUserContext currentUser)
    {
        _platformService = platformService;
        _currentUser = currentUser;
    }

    // ---- Plans ----

    [HttpGet("subscription-plans")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanResponse>>> ListPlans(CancellationToken cancellationToken)
    {
        return Ok(await _platformService.ListPlansAsync(cancellationToken));
    }

    [HttpPost("subscription-plans")]
    [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanResponse>> CreatePlan(
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _platformService.CreatePlanAsync(request, _currentUser.UserId, cancellationToken);
            return CreatedAtAction(nameof(ListPlans), new { id = plan.Id }, plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid plan",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpPost("subscription-plans/{id:guid}/publish")]
    [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanResponse>> PublishPlan(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _platformService.PublishPlanAsync(id, _currentUser.UserId, cancellationToken));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    // ---- Subscriptions ----

    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(IReadOnlyList<PlatformSubscriptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlatformSubscriptionResponse>>> ListSubscriptions(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _platformService.ListSubscriptionsAsync(organizationId, cancellationToken));
    }

    [HttpPut("organizations/{id:guid}/subscription")]
    [ProducesResponseType(typeof(PlatformSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlatformSubscriptionResponse>> AssignSubscription(
        Guid id,
        [FromBody] AssignSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        int? expectedRevision = null;
        if (Request.Headers.TryGetValue("If-Match", out var ifMatch)
            && int.TryParse(ifMatch.ToString().Trim('"'), out var rev))
        {
            expectedRevision = rev;
        }

        try
        {
            var result = await _platformService.AssignSubscriptionAsync(
                id, request, _currentUser.UserId, expectedRevision, cancellationToken);
            return Ok(result);
        }
        catch (ConcurrencyConflictException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Concurrency conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Extensions = { ["code"] = "revision_mismatch" },
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    // ---- Overrides ----

    [HttpPut("organizations/{id:guid}/entitlements/{featureKey}")]
    [ProducesResponseType(typeof(FeatureOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeatureOverrideResponse>> SetFeatureOverride(
        Guid id,
        string featureKey,
        [FromBody] SetFeatureOverrideRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _platformService.SetFeatureOverrideAsync(
                id, featureKey, request, _currentUser.UserId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid override",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    [HttpPut("organizations/{id:guid}/quotas/{quotaKey}")]
    [ProducesResponseType(typeof(QuotaOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuotaOverrideResponse>> SetQuotaOverride(
        Guid id,
        string quotaKey,
        [FromBody] SetQuotaOverrideRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _platformService.SetQuotaOverrideAsync(
                id, quotaKey, request, _currentUser.UserId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid override",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }

    // ---- Audit ----

    [HttpGet("organizations/{id:guid}/subscription-audit")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionAuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubscriptionAuditEventResponse>>> ListAudit(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _platformService.ListAuditEventsAsync(id, cancellationToken));
    }
}
