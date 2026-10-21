using KWinHub.Api.Services;
using KWinHub.Application.Abstractions;
using KWinHub.Application.Subjects;
using KWinHub.Contracts.Subjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly ICurrentUserContext _currentUser;

    public SubjectsController(
        ISubjectService subjectService,
        ICurrentUserContext currentUser)
    {
        _subjectService = subjectService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> List(CancellationToken cancellationToken)
    {
        var subjects = await _subjectService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            cancellationToken);
        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subject = await _subjectService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return subject is null ? NotFound() : Ok(subject);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubjectResponse>> Create(
        [FromBody] CreateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }
}