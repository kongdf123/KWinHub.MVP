using KWinHub.Platform.Organizations;

namespace KWinHub.Platform.Application.Registration;

public interface IOrganizationRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);

    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
