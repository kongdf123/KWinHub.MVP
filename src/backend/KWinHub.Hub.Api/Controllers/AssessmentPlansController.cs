using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Plans;
using KWinHub.Hub.Contracts.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class AssessmentPlansController : ControllerBase
{
    private readonly IAssessmentPlanService _planService;
    private readonly ICurrentUserContext _currentUser;

    public AssessmentPlansController(
        IAssessmentPlanService planService,
        ICurrentUserContext currentUser)
    {
        _planService = planService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssessmentPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssessmentPlanResponse>>> List(CancellationToken cancellationToken)
    {
        var plans = await _planService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssessmentPlanResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _planService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssessmentPlanResponse>> Create(
        [FromBody] CreateAssessmentPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.CreateAsync(
                _currentUser.OrganizationId,
                _currentUser.UserId,
                request,
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
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
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssessmentPlanResponse>> Update(
        Guid id,
        [FromBody] UpdateAssessmentPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.UpdateAsync(
                _currentUser.OrganizationId,
                id,
                request,
                cancellationToken);
            return plan is null ? NotFound() : Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssessmentPlanResponse>> Publish(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.PublishAsync(
                _currentUser.OrganizationId,
                id,
                cancellationToken);
            return plan is null ? NotFound() : Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:guid}/retire")]
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssessmentPlanResponse>> Retire(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.RetireAsync(
                _currentUser.OrganizationId,
                id,
                cancellationToken);
            return plan is null ? NotFound() : Ok(plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:guid}/new-version")]
    [ProducesResponseType(typeof(AssessmentPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssessmentPlanResponse>> CreateNewVersion(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.CreateNewVersionAsync(
                _currentUser.OrganizationId,
                id,
                _currentUser.UserId,
                cancellationToken);
            return plan is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:guid}/completeness")]
    [ProducesResponseType(typeof(PlanCompletenessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanCompletenessResponse>> CheckCompleteness(
        Guid id,
        [FromBody] CheckPlanCompletenessRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _planService.CheckCompletenessAsync(
            _currentUser.OrganizationId,
            id,
            request.SubjectId,
            request.SessionIds,
            cancellationToken);
        return Ok(response);
    }
}