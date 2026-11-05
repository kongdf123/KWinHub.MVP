using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/assets")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ICurrentUserContext _currentUser;

    public AssetsController(
        IAssetService assetService,
        ICurrentUserContext currentUser)
    {
        _assetService = assetService;
        _currentUser = currentUser;
    }

    [HttpGet("by-session/{sessionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AssetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetResponse>>> ListBySession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var assets = await _assetService.ListBySessionAsync(
            _currentUser.OrganizationId,
            sessionId,
            cancellationToken);
        return Ok(assets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return asset is null ? NotFound() : Ok(asset);
    }

    [HttpGet("{id:guid}/playback-credential")]
    [Authorize(Policy = HubPolicyNames.CanPlayback)]
    [ProducesResponseType(typeof(PlaybackCredentialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlaybackCredentialResponse>> RequestPlaybackCredential(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var credential = await _assetService.RequestPlaybackCredentialAsync(
                _currentUser.OrganizationId,
                id,
                cancellationToken);
            return Ok(credential);
        }
        catch (AssetNotFoundException)
        {
            return NotFound();
        }
        catch (AssetNotReadyException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Asset not ready for playback",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("{id:guid}/upload-grant")]
    [ProducesResponseType(typeof(AssetUploadGrantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetUploadGrantResponse>> RequestUploadGrant(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var grant = await _assetService.RequestUploadGrantAsync(
                _currentUser.OrganizationId,
                id,
                cancellationToken);
            return Ok(grant);
        }
        catch (AssetNotFoundException)
        {
            return NotFound();
        }
        catch (AssetAlreadyUploadedException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Asset already uploaded",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("complete")]
    [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetResponse>> CompleteUpload(
        [FromBody] AssetCompletionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var asset = await _assetService.CompleteUploadAsync(
                _currentUser.OrganizationId,
                request,
                cancellationToken);
            return Ok(asset);
        }
        catch (AssetNotFoundException)
        {
            return NotFound();
        }
    }
}