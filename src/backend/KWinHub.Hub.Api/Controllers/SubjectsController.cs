using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Contracts.Subjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

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
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? subjectType,
        CancellationToken cancellationToken)
    {
        var subjects = await _subjectService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            search,
            status,
            subjectType,
            cancellationToken);
        return Ok(subjects);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
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
    [Authorize(Policy = HubPolicyNames.CanWrite)]
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

    [HttpPut("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubjectResponse>> Update(
        Guid id,
        [FromBody] UpdateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await _subjectService.UpdateAsync(
            _currentUser.OrganizationId,
            id,
            request,
            cancellationToken);
        return subject is null ? NotFound() : Ok(subject);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _subjectService.ArchiveAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return archived ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var restored = await _subjectService.RestoreAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return restored ? Ok() : NotFound();
    }
}