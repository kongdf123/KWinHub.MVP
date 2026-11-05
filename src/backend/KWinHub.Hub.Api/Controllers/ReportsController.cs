using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Reports;
using KWinHub.Hub.Contracts.Reports;
using KWinHub.Hub.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserContext _currentUser;

    public ReportsController(
        IReportService reportService,
        ICurrentUserContext currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReportResponse>>> List(CancellationToken cancellationToken)
    {
        var reports = await _reportService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return report is null ? NotFound() : Ok(report);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportDetailResponse>> Generate(
        [FromBody] GenerateReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GenerateAsync(
                _currentUser.OrganizationId,
                _currentUser.UserId,
                request,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{reportId:guid}/playback/{assetId:guid}")]
    [ProducesResponseType(typeof(PlaybackAuthorizationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlaybackAuthorizationResponse>> RequestPlayback(
        Guid reportId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var auth = await _reportService.RequestPlaybackAsync(
            _currentUser.OrganizationId,
            reportId,
            assetId,
            cancellationToken);
        return Ok(auth);
    }
}