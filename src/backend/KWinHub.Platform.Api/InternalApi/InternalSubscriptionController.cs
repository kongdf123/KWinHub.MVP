using KWinHub.Platform.Application.Subscriptions;
using KWinHub.Platform.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Platform.Api.InternalApi;

[ApiController]
[InternalApiKey]
[Route("internal/v1")]
public class InternalSubscriptionController : ControllerBase
{
    private readonly IEntitlementResolver _entitlements;
    private readonly IUsageMeter _usageMeter;

    public InternalSubscriptionController(
        IEntitlementResolver entitlements,
        IUsageMeter usageMeter)
    {
        _entitlements = entitlements;
        _usageMeter = usageMeter;
    }

    [HttpGet("entitlements")]
    public async Task<IActionResult> GetEntitlement(
        [FromQuery] Guid organizationId,
        [FromQuery] string featureKey,
        CancellationToken cancellationToken)
    {
        var result = await _entitlements.ResolveAsync(organizationId, featureKey, cancellationToken);
        return Ok(new
        {
            featureKey = result.FeatureKey,
            allowed = result.Enabled,
            reason = result.Reason
        });
    }

    [HttpPost("usage/consume")]
    public async Task<IActionResult> ConsumeUsage(
        [FromBody] ConsumeUsageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var balance = await _usageMeter.ConsumeAsync(
                request.OrganizationId,
                request.MetricKey,
                request.BusinessEventId,
                request.Delta,
                cancellationToken);

            return Ok(new { balance });
        }
        catch (QuotaExceededException ex)
        {
            return Conflict(new
            {
                error = "quota_exceeded",
                metricKey = ex.MetricKey,
                consumed = ex.Consumed,
                limit = ex.Limit,
                requested = ex.Requested,
                message = ex.Message
            });
        }
    }
}

public sealed record ConsumeUsageRequest(
    Guid OrganizationId,
    string MetricKey,
    string BusinessEventId,
    long Delta);