using KWinHub.Hub.Organizations;

namespace KWinHub.Hub.Application.Organizations;

public interface IOrganizationInvitationRepository
{
    Task<OrganizationInvitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<OrganizationInvitation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationInvitation>> ListByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationInvitation invitation,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        OrganizationInvitation invitation,
        CancellationToken cancellationToken = default);
}