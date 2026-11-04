using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Contracts.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class DataSourcesController : ControllerBase
{
    private readonly IDataSourceService _dataSourceService;
    private readonly ICurrentUserContext _currentUser;

    public DataSourcesController(
        IDataSourceService dataSourceService,
        ICurrentUserContext currentUser)
    {
        _dataSourceService = dataSourceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DataSourceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DataSourceResponse>>> List(CancellationToken cancellationToken)
    {
        var sources = await _dataSourceService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(sources);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DataSourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataSourceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var source = await _dataSourceService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DataSourceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<DataSourceResponse>> Create(
        [FromBody] CreateDataSourceRequest request,
        CancellationToken cancellationToken)
    {
        var source = await _dataSourceService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = source.Id }, source);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(DataSourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataSourceResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var source = await _dataSourceService.DeactivateAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }
}