using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Contracts.Organizations;
using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Organizations;

public class OrganizationMemberService : IOrganizationMemberService
{
    private static readonly TimeSpan DefaultInvitationValidity = TimeSpan.FromDays(7);

    private readonly IMembershipRepository _memberships;
    private readonly IOrganizationInvitationRepository _invitations;

    public OrganizationMemberService(
        IMembershipRepository memberships,
        IOrganizationInvitationRepository invitations)
    {
        _memberships = memberships;
        _invitations = invitations;
    }

    public async Task<IReadOnlyList<OrganizationMemberResponse>> ListMembersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var memberships = await _memberships.ListByOrganizationAsync(organizationId, cancellationToken);
        return memberships
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new OrganizationMemberResponse(
                UserId: m.UserId,
                Role: m.Role,
                Status: m.Status,
                JoinedAt: m.AcceptedAt ?? m.CreatedAt,
                LastActiveAt: m.UpdatedAt))
            .ToList();
    }

    public async Task<OrganizationMemberResponse?> ChangeRoleAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetAsync(userId, organizationId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        membership.ChangeRole(role);
        await _memberships.UpdateAsync(membership, cancellationToken);
        return ToMemberResponse(membership);
    }

    public async Task<OrganizationMemberResponse?> SuspendAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetAsync(userId, organizationId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        membership.Suspend();
        await _memberships.UpdateAsync(membership, cancellationToken);
        return ToMemberResponse(membership);
    }

    public async Task<OrganizationMemberResponse?> ReactivateAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetAsync(userId, organizationId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        membership.Reactivate();
        await _memberships.UpdateAsync(membership, cancellationToken);
        return ToMemberResponse(membership);
    }

    public async Task<OrganizationInvitationResponse> InviteAsync(
        Guid organizationId,
        string email,
        string role,
        Guid invitedBy,
        CancellationToken cancellationToken = default)
    {
        var invitation = new OrganizationInvitation(
            organizationId: organizationId,
            email: email,
            role: role,
            createdBy: invitedBy,
            validity: DefaultInvitationValidity);

        await _invitations.AddAsync(invitation, cancellationToken);
        return ToInvitationResponse(invitation);
    }

    public async Task<IReadOnlyList<OrganizationInvitationResponse>> ListInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var invitations = await _invitations.ListByOrganizationAsync(
            organizationId,
            status,
            cancellationToken);

        return invitations
            .OrderByDescending(i => i.CreatedAt)
            .Select(ToInvitationResponse)
            .ToList();
    }

    public async Task<OrganizationInvitationResponse?> AcceptInvitationAsync(
        string token,
        Guid acceptedBy,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _invitations.GetByTokenAsync(token, cancellationToken);
        if (invitation is null)
        {
            return null;
        }

        if (invitation.IsAccepted)
        {
            return ToInvitationResponse(invitation);
        }

        try
        {
            invitation.Accept(acceptedBy);
        }
        catch (InvalidOperationException)
        {
            await _invitations.UpdateAsync(invitation, cancellationToken);
            throw;
        }

        var existingMembership = await _memberships.GetAsync(
            acceptedBy,
            invitation.OrganizationId,
            cancellationToken);

        if (existingMembership is null)
        {
            var membership = new OrganizationMembership(
                userId: acceptedBy,
                organizationId: invitation.OrganizationId,
                role: invitation.Role);
            membership.AcceptInvite();
            await _memberships.AddAsync(membership, cancellationToken);
        }
        else if (!existingMembership.IsActive)
        {
            existingMembership.Reactivate();
            existingMembership.ChangeRole(invitation.Role);
            await _memberships.UpdateAsync(existingMembership, cancellationToken);
        }

        await _invitations.UpdateAsync(invitation, cancellationToken);
        return ToInvitationResponse(invitation);
    }

    public async Task<OrganizationInvitationResponse?> RevokeInvitationAsync(
        Guid organizationId,
        Guid invitationId,
        Guid revokedBy,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _invitations.GetByIdAsync(invitationId, cancellationToken);
        if (invitation is null || invitation.OrganizationId != organizationId)
        {
            return null;
        }

        invitation.Revoke(revokedBy);
        await _invitations.UpdateAsync(invitation, cancellationToken);
        return ToInvitationResponse(invitation);
    }

    private static OrganizationMemberResponse ToMemberResponse(OrganizationMembership m) =>
        new(
            UserId: m.UserId,
            Role: m.Role,
            Status: m.Status,
            JoinedAt: m.AcceptedAt ?? m.CreatedAt,
            LastActiveAt: m.UpdatedAt);

    private static OrganizationInvitationResponse ToInvitationResponse(OrganizationInvitation i) =>
        new(
            Id: i.Id,
            OrganizationId: i.OrganizationId,
            Email: i.Email,
            Role: i.Role,
            Status: i.Status,
            ExpiresAt: i.ExpiresAt,
            CreatedBy: i.CreatedBy,
            CreatedAt: i.CreatedAt,
            AcceptedAt: i.AcceptedAt,
            AcceptedBy: i.AcceptedBy,
            RevokedAt: i.RevokedAt,
            RevokedBy: i.RevokedBy);
}