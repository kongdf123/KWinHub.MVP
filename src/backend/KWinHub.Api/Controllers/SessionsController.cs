using KWinHub.Api.Services;
using KWinHub.Application.Abstractions;
using KWinHub.Application.Sessions;
using KWinHub.Contracts.Sessions;
using KWinHub.Domain.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserContext _currentUser;

    public SessionsController(
        ISessionService sessionService,
        ICurrentUserContext currentUser)
    {
        _sessionService = sessionService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> List(CancellationToken cancellationToken)
    {
        var sessions = await _sessionService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("by-subject/{subjectId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> ListBySubject(
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionService.ListBySubjectAsync(
            _currentUser.OrganizationId,
            subjectId,
            cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(SessionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionStatusResponse>> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var status = await _sessionService.GetStatusAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SessionResponse>> Create(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionService.GetOrCreateByExternalIdAsync(
                _currentUser.OrganizationId,
                request,
                cancellationToken);

            if (session.CreatedAt == session.UpdatedAt || session.Status == "Received")
            {
                return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
            }

            return Ok(session);
        }
        catch (QuotaExceededException ex)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Quota exceeded",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Extensions =
                {
                    ["code"] = "quota_exceeded",
                    ["metric"] = ex.MetricKey,
                    ["consumed"] = ex.Consumed,
                    ["limit"] = ex.Limit,
                },
            });
        }
    }
}
