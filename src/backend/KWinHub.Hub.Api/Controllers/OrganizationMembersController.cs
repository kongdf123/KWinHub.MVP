using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Organizations;
using KWinHub.Hub.Contracts.Organizations;
using KWinHub.Hub.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/members")]
public class OrganizationMembersController : ControllerBase
{
    private readonly IOrganizationMemberService _memberService;
    private readonly ICurrentUserContext _currentUser;

    public OrganizationMembersController(
        IOrganizationMemberService memberService,
        ICurrentUserContext currentUser)
    {
        _memberService = memberService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganizationMemberResponse>>> List(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var members = await _memberService.ListMembersAsync(organizationId, cancellationToken);
        return Ok(members);
    }

    [HttpPut("{userId:guid}/role")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(OrganizationMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationMemberResponse>> ChangeRole(
        Guid organizationId,
        Guid userId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!OrganizationRoles.IsValid(request.Role))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid role",
                Detail = $"Role must be one of: {string.Join(", ", OrganizationRoles.All)}",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var member = await _memberService.ChangeRoleAsync(
            organizationId,
            userId,
            request.Role,
            cancellationToken);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost("{userId:guid}/suspend")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(OrganizationMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationMemberResponse>> Suspend(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await _memberService.SuspendAsync(organizationId, userId, cancellationToken);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpPost("{userId:guid}/reactivate")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(OrganizationMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationMemberResponse>> Reactivate(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await _memberService.ReactivateAsync(organizationId, userId, cancellationToken);
        return member is null ? NotFound() : Ok(member);
    }

    [HttpGet("invitations")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationInvitationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganizationInvitationResponse>>> ListInvitations(
        Guid organizationId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var invitations = await _memberService.ListInvitationsAsync(
            organizationId,
            status,
            cancellationToken);
        return Ok(invitations);
    }

    [HttpPost("invitations")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(OrganizationInvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganizationInvitationResponse>> Invite(
        Guid organizationId,
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid email",
                Detail = "Email is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!OrganizationRoles.IsValid(request.Role))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid role",
                Detail = $"Role must be one of: {string.Join(", ", OrganizationRoles.All)}",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var invitation = await _memberService.InviteAsync(
            organizationId,
            request.Email,
            request.Role,
            _currentUser.UserId,
            cancellationToken);

        return CreatedAtAction(
            nameof(ListInvitations),
            new { organizationId },
            invitation);
    }

    [HttpPost("invitations/{invitationId:guid}/revoke")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(typeof(OrganizationInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationInvitationResponse>> RevokeInvitation(
        Guid organizationId,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        var invitation = await _memberService.RevokeInvitationAsync(
            organizationId,
            invitationId,
            _currentUser.UserId,
            cancellationToken);
        return invitation is null ? NotFound() : Ok(invitation);
    }

    [HttpPost("invitations/accept")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrganizationInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationInvitationResponse>> AcceptInvitation(
        Guid organizationId,
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid token",
                Detail = "Invitation token is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        try
        {
            var invitation = await _memberService.AcceptInvitationAsync(
                request.Token,
                _currentUser.UserId,
                cancellationToken);
            return invitation is null ? NotFound() : Ok(invitation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Cannot accept invitation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}