using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ICurrentUserContext _currentUser;

    public DevicesController(
        IDeviceService deviceService,
        ICurrentUserContext currentUser)
    {
        _deviceService = deviceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeviceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeviceResponse>>> List(CancellationToken cancellationToken)
    {
        var devices = await _deviceService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(devices);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<DeviceResponse>> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var device = await _deviceService.DeactivateAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return device is null ? NotFound() : Ok(device);
    }
}