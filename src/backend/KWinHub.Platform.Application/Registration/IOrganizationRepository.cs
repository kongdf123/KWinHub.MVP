using KWinHub.Domain.Organizations;

namespace KWinHub.Application.Registration;

public interface IOrganizationRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);

    Task AddMembershipAsync(OrganizationMembership membership, CancellationToken cancellationToken = default);
}
