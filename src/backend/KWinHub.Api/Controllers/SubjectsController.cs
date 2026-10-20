using KWinHub.Application.Subjects;
using KWinHub.Contracts.Subjects;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> List(
        [FromHeader(Name = "X-Organization-Id")] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var subjects = await _subjectService.ListByOrganizationAsync(organizationId, cancellationToken);
        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subject = await _subjectService.GetByIdAsync(id, cancellationToken);
        return subject is null ? NotFound() : Ok(subject);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubjectResponse>> Create(
        [FromHeader(Name = "X-Organization-Id")] Guid organizationId,
        [FromBody] CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.CreateAsync(organizationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }
}