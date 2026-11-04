using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Services;
using KWinHub.Hub.Application.Ingestion;
using KWinHub.Hub.Contracts.Ingestion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly IIngestionService _ingestionService;
    private readonly ICurrentUserContext _currentUser;

    public IngestionController(
        IIngestionService ingestionService,
        ICurrentUserContext currentUser)
    {
        _ingestionService = ingestionService;
        _currentUser = currentUser;
    }

    [HttpPost("sessions")]
    [ProducesResponseType(typeof(IngestSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IngestSessionResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(IngestSessionResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IngestSessionResponse>> IngestSession(
        [FromBody] IngestSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _ingestionService.IngestSessionAsync(
                _currentUser.OrganizationId,
                request,
                cancellationToken);
            return Ok(response);
        }
        catch (IngestionException ex)
        {
            var errors = new List<IngestionError>
            {
                new(ex.ErrorCode, ex.Message, ex.Field)
            };
            var failedResponse = new IngestSessionResponse(
                SessionId: Guid.Empty,
                SessionExternalId: null,
                Status: "Failed",
                Trials: null,
                AssetPlaceholders: null,
                Errors: errors);

            return ex.ErrorCode == IngestionErrorCodes.Conflict
                ? Conflict(failedResponse)
                : UnprocessableEntity(failedResponse);
        }
    }

    [HttpGet("operations/{clientOperationId}")]
    [ProducesResponseType(typeof(IngestionOperationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngestionOperationStatusResponse>> GetOperationStatus(
        string clientOperationId,
        CancellationToken cancellationToken)
    {
        var status = await _ingestionService.GetOperationStatusAsync(
            _currentUser.OrganizationId,
            clientOperationId,
            cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpGet("sessions/{sessionId:guid}/status")]
    [ProducesResponseType(typeof(IngestionOperationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngestionOperationStatusResponse>> GetSessionStatus(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var status = await _ingestionService.GetSessionStatusAsync(
            _currentUser.OrganizationId,
            sessionId,
            cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }
}