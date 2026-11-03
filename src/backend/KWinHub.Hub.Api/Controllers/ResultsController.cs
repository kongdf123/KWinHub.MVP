using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Services;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Contracts.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IResultService _resultService;
    private readonly ICurrentUserContext _currentUser;

    public ResultsController(
        IResultService resultService,
        ICurrentUserContext currentUser)
    {
        _resultService = resultService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResultResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResultResponse>>> List(CancellationToken cancellationToken)
    {
        var results = await _resultService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _resultService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("session/{sessionId:guid}")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultResponse>> GetBySession(Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _resultService.GetBySessionAsync(
            _currentUser.OrganizationId,
            sessionId,
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}