using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Contracts.Subjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SourceIdentityMappingsController : ControllerBase
{
    private readonly ISourceIdentityMappingService _mappingService;
    private readonly ICurrentUserContext _currentUser;

    public SourceIdentityMappingsController(
        ISourceIdentityMappingService mappingService,
        ICurrentUserContext currentUser)
    {
        _mappingService = mappingService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<SourceIdentityMappingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SourceIdentityMappingResponse>>> List(
        [FromQuery] string? status,
        [FromQuery] string? sourceApp,
        CancellationToken cancellationToken)
    {
        var mappings = await _mappingService.ListAsync(
            _currentUser.OrganizationId,
            status,
            sourceApp,
            cancellationToken);
        return Ok(mappings);
    }

    [HttpGet("by-source")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(SourceIdentityMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SourceIdentityMappingResponse>> GetBySourceIdentity(
        [FromQuery] string sourceApp,
        [FromQuery] string installationScope,
        [FromQuery] string externalId,
        CancellationToken cancellationToken)
    {
        var mapping = await _mappingService.GetBySourceIdentityAsync(
            _currentUser.OrganizationId,
            sourceApp,
            installationScope,
            externalId,
            cancellationToken);

        return mapping is null ? NotFound() : Ok(mapping);
    }

    [HttpGet("by-subject/{subjectId:guid}")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<SourceIdentityMappingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SourceIdentityMappingResponse>>> ListBySubject(
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var mappings = await _mappingService.ListBySubjectAsync(
            _currentUser.OrganizationId,
            subjectId,
            cancellationToken);
        return Ok(mappings);
    }

    [HttpPost]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(SourceIdentityMappingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SourceIdentityMappingResponse>> Register(
        [FromBody] CreateSourceIdentityMappingRequest request,
        CancellationToken cancellationToken)
    {
        var mapping = await _mappingService.RegisterAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetBySourceIdentity),
            new
            {
                sourceApp = mapping.SourceApp,
                installationScope = mapping.InstallationScope,
                externalId = mapping.ExternalId
            },
            mapping);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(SourceIdentityMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SourceIdentityMappingResponse>> ResolveConflict(
        Guid id,
        [FromBody] ResolveSourceIdentityConflictRequest request,
        CancellationToken cancellationToken)
    {
        var mapping = await _mappingService.ResolveConflictAsync(
            _currentUser.OrganizationId,
            id,
            request,
            _currentUser.UserId,
            cancellationToken);

        return mapping is null ? NotFound() : Ok(mapping);
    }
}