using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Contracts.Protocols;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class MetricDefinitionsController : ControllerBase
{
    private readonly IMetricDefinitionService _metricDefinitionService;
    private readonly ICurrentUserContext _currentUser;

    public MetricDefinitionsController(
        IMetricDefinitionService metricDefinitionService,
        ICurrentUserContext currentUser)
    {
        _metricDefinitionService = metricDefinitionService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MetricDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MetricDefinitionResponse>>> List(CancellationToken cancellationToken)
    {
        var definitions = await _metricDefinitionService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(definitions);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MetricDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MetricDefinitionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var definition = await _metricDefinitionService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return definition is null ? NotFound() : Ok(definition);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MetricDefinitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MetricDefinitionResponse>> Create(
        [FromBody] CreateMetricDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _metricDefinitionService.CreateAsync(
                _currentUser.OrganizationId,
                request,
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = definition.Id }, definition);
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MetricDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MetricDefinitionResponse>> Update(
        Guid id,
        [FromBody] UpdateMetricDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _metricDefinitionService.UpdateAsync(
                _currentUser.OrganizationId,
                id,
                request,
                cancellationToken);
            return definition is null ? NotFound() : Ok(definition);
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

    [HttpPost("{id:guid}/deprecate")]
    [ProducesResponseType(typeof(MetricDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MetricDefinitionResponse>> Deprecate(Guid id, CancellationToken cancellationToken)
    {
        var definition = await _metricDefinitionService.DeprecateAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return definition is null ? NotFound() : Ok(definition);
    }
}