using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Teams;
using KWinHub.Hub.Contracts.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ICurrentUserContext _currentUser;

    public TeamsController(
        ITeamService teamService,
        ICurrentUserContext currentUser)
    {
        _teamService = teamService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<TeamResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var teams = await _teamService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            search,
            status,
            cancellationToken);
        return Ok(teams);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var team = await _teamService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamResponse>> Create(
        [FromBody] CreateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var team = await _teamService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamResponse>> Update(
        Guid id,
        [FromBody] UpdateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var team = await _teamService.UpdateAsync(
            _currentUser.OrganizationId,
            id,
            request,
            cancellationToken);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _teamService.ArchiveAsync(
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
        var restored = await _teamService.RestoreAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return restored ? Ok() : NotFound();
    }

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<TeamMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TeamMemberResponse>>> ListMembers(
        Guid id,
        CancellationToken cancellationToken)
    {
        var team = await _teamService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        if (team is null)
        {
            return NotFound();
        }

        var members = await _teamService.ListMembersAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return Ok(members);
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(TeamMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberResponse>> AddMember(
        Guid id,
        [FromBody] AddTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        var member = await _teamService.AddMemberAsync(
            _currentUser.OrganizationId,
            id,
            request,
            cancellationToken);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpDelete("{id:guid}/members/{subjectId:guid}")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid id,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var removed = await _teamService.RemoveMemberAsync(
            _currentUser.OrganizationId,
            id,
            subjectId,
            cancellationToken);
        return removed ? Ok() : NotFound();
    }
}