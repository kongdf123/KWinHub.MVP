using System.Security.Claims;
using KWinHub.Application.Abstractions;
using KWinHub.Application.Catalogs;
using KWinHub.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/platform/catalogs")]
public class PlatformCatalogsController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly ICurrentUserContext _currentUser;

    public PlatformCatalogsController(
        ICatalogService catalogService,
        ICurrentUserContext currentUser)
    {
        _catalogService = catalogService;
        _currentUser = currentUser;
    }

    [HttpGet("protocols")]
    public async Task<ActionResult<IReadOnlyList<GlobalProtocolResponse>>> ListProtocols(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogService.ListProtocolsAsync(cancellationToken));
    }

    [HttpPost("protocols")]
    [ProducesResponseType(typeof(GlobalProtocolResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<GlobalProtocolResponse>> CreateProtocol(
        [FromBody] UpsertGlobalProtocolRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalogService.CreateProtocolAsync(
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return Created($"/api/v1/platform/catalogs/protocols/{created.Id}", created);
    }

    [HttpPut("protocols/{id:guid}")]
    [ProducesResponseType(typeof(GlobalProtocolResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GlobalProtocolResponse>> UpdateProtocol(
        Guid id,
        [FromBody] UpsertGlobalProtocolRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateProtocolAsync(
            id,
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("protocols/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeProtocolStatus(
        Guid id,
        [FromBody] UpdateCatalogStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _catalogService.ChangeProtocolStatusAsync(
                id,
                request.Action,
                _currentUser.UserId,
                ActorEmail,
                IpAddress,
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return InvalidAction(ex);
        }
    }

    [HttpDelete("protocols/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProtocol(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _catalogService.DeleteProtocolAsync(
            id,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<IReadOnlyList<GlobalMetricResponse>>> ListMetrics(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogService.ListMetricsAsync(cancellationToken));
    }

    [HttpPost("metrics")]
    [ProducesResponseType(typeof(GlobalMetricResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<GlobalMetricResponse>> CreateMetric(
        [FromBody] UpsertGlobalMetricRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalogService.CreateMetricAsync(
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return Created($"/api/v1/platform/catalogs/metrics/{created.Id}", created);
    }

    [HttpPut("metrics/{id:guid}")]
    [ProducesResponseType(typeof(GlobalMetricResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GlobalMetricResponse>> UpdateMetric(
        Guid id,
        [FromBody] UpsertGlobalMetricRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateMetricAsync(
            id,
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("metrics/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeMetricStatus(
        Guid id,
        [FromBody] UpdateCatalogStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _catalogService.ChangeMetricStatusAsync(
                id,
                request.Action,
                _currentUser.UserId,
                ActorEmail,
                IpAddress,
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return InvalidAction(ex);
        }
    }

    [HttpDelete("metrics/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMetric(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _catalogService.DeleteMetricAsync(
            id,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("devices")]
    public async Task<ActionResult<IReadOnlyList<DeviceModelResponse>>> ListDevices(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogService.ListDevicesAsync(cancellationToken));
    }

    [HttpPost("devices")]
    [ProducesResponseType(typeof(DeviceModelResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<DeviceModelResponse>> CreateDevice(
        [FromBody] UpsertDeviceModelRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalogService.CreateDeviceAsync(
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return Created($"/api/v1/platform/catalogs/devices/{created.Id}", created);
    }

    [HttpPut("devices/{id:guid}")]
    [ProducesResponseType(typeof(DeviceModelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceModelResponse>> UpdateDevice(
        Guid id,
        [FromBody] UpsertDeviceModelRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateDeviceAsync(
            id,
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("devices/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeDeviceStatus(
        Guid id,
        [FromBody] UpdateCatalogStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _catalogService.ChangeDeviceStatusAsync(
                id,
                request.Action,
                _currentUser.UserId,
                ActorEmail,
                IpAddress,
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return InvalidAction(ex);
        }
    }

    [HttpDelete("devices/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDevice(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _catalogService.DeleteDeviceAsync(
            id,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("algorithms")]
    public async Task<ActionResult<IReadOnlyList<AlgorithmVersionResponse>>> ListAlgorithms(
        CancellationToken cancellationToken)
    {
        return Ok(await _catalogService.ListAlgorithmsAsync(cancellationToken));
    }

    [HttpPost("algorithms")]
    [ProducesResponseType(typeof(AlgorithmVersionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AlgorithmVersionResponse>> CreateAlgorithm(
        [FromBody] UpsertAlgorithmVersionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _catalogService.CreateAlgorithmAsync(
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return Created($"/api/v1/platform/catalogs/algorithms/{created.Id}", created);
    }

    [HttpPut("algorithms/{id:guid}")]
    [ProducesResponseType(typeof(AlgorithmVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlgorithmVersionResponse>> UpdateAlgorithm(
        Guid id,
        [FromBody] UpsertAlgorithmVersionRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateAlgorithmAsync(
            id,
            request,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("algorithms/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeAlgorithmStatus(
        Guid id,
        [FromBody] UpdateCatalogStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _catalogService.ChangeAlgorithmStatusAsync(
                id,
                request.Action,
                _currentUser.UserId,
                ActorEmail,
                IpAddress,
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return InvalidAction(ex);
        }
    }

    [HttpDelete("algorithms/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAlgorithm(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _catalogService.DeleteAlgorithmAsync(
            id,
            _currentUser.UserId,
            ActorEmail,
            IpAddress,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    private string? ActorEmail =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;

    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    private IActionResult InvalidAction(ArgumentException ex) =>
        BadRequest(new ProblemDetails
        {
            Title = "Invalid status action",
            Detail = ex.Message,
            Status = StatusCodes.Status400BadRequest
        });
}
