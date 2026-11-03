using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Services;
using KWinHub.Hub.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrgContextService _orgContext;
    private readonly ICurrentUserContext _currentUser;

    public OrganizationsController(
        IOrgContextService orgContext,
        ICurrentUserContext currentUser)
    {
        _orgContext = orgContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserOrganizationSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserOrganizationSummary>>> List(
        CancellationToken cancellationToken)
    {
        var orgs = await _orgContext.ListUserOrganizationsAsync(
            _currentUser.UserId,
            cancellationToken);
        return Ok(orgs);
    }

    [HttpGet("{organizationId:guid}/context")]
    [ProducesResponseType(typeof(OrgContextResolution), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrgContextResolution>> GetContext(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var context = await _orgContext.ResolveAsync(
            _currentUser.UserId,
            organizationId,
            cancellationToken);

        if (context is null || !context.IsActiveMember)
        {
            return Forbid();
        }

        if (!context.IsOrganizationActive)
        {
            return Forbid();
        }

        return Ok(context);
    }
}