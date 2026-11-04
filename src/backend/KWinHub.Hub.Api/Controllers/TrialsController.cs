using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class TrialsController : ControllerBase
{
    private readonly ITrialService _trialService;
    private readonly ICurrentUserContext _currentUser;

    public TrialsController(
        ITrialService trialService,
        ICurrentUserContext currentUser)
    {
        _trialService = trialService;
        _currentUser = currentUser;
    }

    [HttpGet("by-session/{sessionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<TrialResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TrialResponse>>> ListBySession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var trials = await _trialService.ListBySessionAsync(
            _currentUser.OrganizationId,
            sessionId,
            cancellationToken);
        return Ok(trials);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrialResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trial = await _trialService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return trial is null ? NotFound() : Ok(trial);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TrialResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrialResponse>> Create(
        [FromBody] CreateTrialRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var trial = await _trialService.CreateAsync(
                _currentUser.OrganizationId,
                request,
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = trial.Id }, trial);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(TrialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrialResponse>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var trial = await _trialService.CompleteAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return trial is null ? NotFound() : Ok(trial);
    }

    [HttpPost("{id:guid}/fail")]
    [ProducesResponseType(typeof(TrialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrialResponse>> Fail(Guid id, CancellationToken cancellationToken)
    {
        var trial = await _trialService.FailAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return trial is null ? NotFound() : Ok(trial);
    }
}