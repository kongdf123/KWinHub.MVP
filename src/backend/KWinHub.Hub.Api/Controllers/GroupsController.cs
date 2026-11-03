using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Contracts.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ICurrentUserContext _currentUser;

    public GroupsController(
        IGroupService groupService,
        ICurrentUserContext currentUser)
    {
        _groupService = groupService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<GroupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? groupType,
        CancellationToken cancellationToken)
    {
        var groups = await _groupService.ListByOrganizationAsync(
            _currentUser.OrganizationId,
            search,
            status,
            groupType,
            cancellationToken);
        return Ok(groups);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await _groupService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpPost]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupResponse>> Create(
        [FromBody] CreateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.CreateAsync(
            _currentUser.OrganizationId,
            request,
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(GroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GroupResponse>> Update(
        Guid id,
        [FromBody] UpdateGroupRequest request,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.UpdateAsync(
            _currentUser.OrganizationId,
            id,
            request,
            cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _groupService.ArchiveAsync(
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
        var restored = await _groupService.RestoreAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return restored ? Ok() : NotFound();
    }

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<GroupMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<GroupMemberResponse>>> ListMembers(
        Guid id,
        CancellationToken cancellationToken)
    {
        var group = await _groupService.GetByIdAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        var members = await _groupService.ListMembersAsync(
            _currentUser.OrganizationId,
            id,
            cancellationToken);
        return Ok(members);
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(GroupMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupMemberResponse>> AddMember(
        Guid id,
        [FromBody] AddGroupMemberRequest request,
        CancellationToken cancellationToken)
    {
        var member = await _groupService.AddMemberAsync(
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
        var removed = await _groupService.RemoveMemberAsync(
            _currentUser.OrganizationId,
            id,
            subjectId,
            cancellationToken);
        return removed ? Ok() : NotFound();
    }
}