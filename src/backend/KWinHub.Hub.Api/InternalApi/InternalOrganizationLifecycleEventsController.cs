using System.Text.Json;
using KWinHub.Hub.Application.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.InternalApi;

[ApiController]
[InternalApiKey]
[Route("internal/v1/organization-lifecycle-events")]
public class InternalOrganizationLifecycleEventsController : ControllerBase
{
    private readonly IOrganizationLifecycleEventConsumer _consumer;

    public InternalOrganizationLifecycleEventsController(IOrganizationLifecycleEventConsumer consumer)
    {
        _consumer = consumer;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] LifecycleEventRequest request,
        CancellationToken cancellationToken)
    {
        string status;
        try
        {
            var payload = JsonSerializer.Deserialize<LifecycleEventPayload>(request.PayloadJson);
            status = payload?.Status ?? "Active";
        }
        catch
        {
            status = "Active";
        }

        await _consumer.ConsumeAsync(
            request.EventId,
            request.OrganizationId,
            request.EventType,
            request.EventVersion,
            status,
            cancellationToken);

        return Ok(new { accepted = true });
    }
}

public sealed record LifecycleEventRequest(
    Guid EventId,
    Guid OrganizationId,
    string EventType,
    long EventVersion,
    string PayloadJson);

public sealed record LifecycleEventPayload(
    Guid OrganizationId,
    string EventType,
    long Version,
    string Status,
    DateTimeOffset OccurredAt);