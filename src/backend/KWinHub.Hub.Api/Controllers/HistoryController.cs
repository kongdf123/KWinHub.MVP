using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.History;
using KWinHub.Hub.Contracts.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/history")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    private readonly ICurrentUserContext _currentUser;

    public HistoryController(
        IHistoryService historyService,
        ICurrentUserContext currentUser)
    {
        _historyService = historyService;
        _currentUser = currentUser;
    }

    [HttpGet("subjects/{subjectId:guid}")]
    [ProducesResponseType(typeof(HistoryListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HistoryListResponse>> ListSubjectHistory(
        Guid subjectId,
        [FromQuery] Guid? protocolId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var filters = new HistoryFilters(protocolId, from, to, status);
        var history = await _historyService.ListSubjectHistoryAsync(
            _currentUser.OrganizationId,
            subjectId,
            filters,
            cancellationToken);
        return Ok(history);
    }

    [HttpGet("subjects/{subjectId:guid}/comparison")]
    [ProducesResponseType(typeof(ComparisonResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComparisonResponse>> Compare(
        Guid subjectId,
        [FromQuery] Guid[] sessionIds,
        CancellationToken cancellationToken)
    {
        var comparison = await _historyService.CompareSessionsAsync(
            _currentUser.OrganizationId,
            subjectId,
            sessionIds,
            cancellationToken);
        return Ok(comparison);
    }
}