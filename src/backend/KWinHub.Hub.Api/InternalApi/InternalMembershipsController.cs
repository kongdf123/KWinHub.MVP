using KWinHub.Hub.Application.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.InternalApi;

[ApiController]
[InternalApiKey]
[Route("internal/v1")]
public class InternalMembershipsController : ControllerBase
{
    private readonly IMembershipService _memberships;

    public InternalMembershipsController(IMembershipService memberships)
    {
        _memberships = memberships;
    }

    [HttpPost("memberships")]
    public async Task<IActionResult> Provision(
        [FromBody] ProvisionMembershipRequest request,
        CancellationToken cancellationToken)
    {
        await _memberships.ProvisionAsync(
            request.OrganizationId,
            request.UserId,
            request.Role,
            cancellationToken);

        return Ok(new { provisioned = true });
    }

    [HttpGet("users/{userId:guid}/primary-membership")]
    public async Task<IActionResult> GetPrimary(Guid userId, CancellationToken cancellationToken)
    {
        var membership = await _memberships.GetPrimaryByUserAsync(userId, cancellationToken);
        if (membership is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            organizationId = membership.OrganizationId,
            role = membership.Role
        });
    }

    [HttpGet("organizations/{organizationId:guid}/memberships")]
    public async Task<IActionResult> ListByOrganization(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var memberships = await _memberships.ListByOrganizationAsync(organizationId, cancellationToken);

        return Ok(memberships.Select(m => new
        {
            userId = m.UserId,
            role = m.Role,
            status = m.Status,
            joinedAt = m.CreatedAt
        }));
    }
}

public sealed record ProvisionMembershipRequest(Guid OrganizationId, Guid UserId, string Role);
