using KWinHub.Application.Abstractions;
using KWinHub.Application.Protocols;
using KWinHub.Contracts.Protocols;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ProtocolsController : ControllerBase
{
    private readonly IProtocolService _protocolService;
    private readonly ICurrentUserContext _currentUser;

    public ProtocolsController(
        IProtocolService protocolService,
        ICurrentUserContext currentUser)
    {
        _protocolService = protocolService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProtocolResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProtocolResponse>>> List(CancellationToken cancellationToken)
    {
        var protocols = await _protocolService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(protocols);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProtocolResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProtocolResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var protocol = await _protocolService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return protocol is null ? NotFound() : Ok(protocol);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProtocolResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProtocolResponse>> Create(
        [FromBody] CreateProtocolRequest request,
        CancellationToken cancellationToken)
    {
        var protocol = await _protocolService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = protocol.Id }, protocol);
    }
}
