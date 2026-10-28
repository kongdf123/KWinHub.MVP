using KWinHub.Application.Authorization;
using KWinHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly KWinHubDbContext _db;

    public MembershipRepository(KWinHubDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.OrganizationMemberships.AnyAsync(
            m => m.UserId == userId
                 && m.OrganizationId == organizationId
                 && m.Status == "Active",
            cancellationToken);
    }
}
