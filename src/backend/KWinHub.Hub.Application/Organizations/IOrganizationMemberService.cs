using KWinHub.Hub.Contracts.Organizations;

namespace KWinHub.Hub.Application.Organizations;

public interface IOrganizationMemberService
{
    Task<IReadOnlyList<OrganizationMemberResponse>> ListMembersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<OrganizationMemberResponse?> ChangeRoleAsync(
        Guid organizationId,
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<OrganizationMemberResponse?> SuspendAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OrganizationMemberResponse?> ReactivateAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OrganizationInvitationResponse> InviteAsync(
        Guid organizationId,
        string email,
        string role,
        Guid invitedBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationInvitationResponse>> ListInvitationsAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<OrganizationInvitationResponse?> AcceptInvitationAsync(
        string token,
        Guid acceptedBy,
        CancellationToken cancellationToken = default);

    Task<OrganizationInvitationResponse?> RevokeInvitationAsync(
        Guid organizationId,
        Guid invitationId,
        Guid revokedBy,
        CancellationToken cancellationToken = default);
}