using KWinHub.Domain.Organizations;
using KWinHub.Domain.Protocols;
using KWinHub.Domain.Subjects;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    // Deterministic IDs used by DevAuth token issuer and frontend fixtures
    public static readonly Guid DemoOrganizationId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoUserId         = new("22222222-2222-2222-2222-222222222222");

    public static async Task EnsureDevelopmentSeedDataAsync(KWinHubDbContext db, CancellationToken ct = default)
    {
        // Organization
        if (!await db.Organizations.AnyAsync(ct))
        {
            var org = new Organization(
                name: "Kunwei Demo Organization",
                slug: "kunwei-demo",
                id: DemoOrganizationId);
            await db.Organizations.AddAsync(org, ct);

            var membership = new OrganizationMembership(
                userId: DemoUserId,
                organizationId: DemoOrganizationId,
                role: "Owner");
            await db.OrganizationMemberships.AddAsync(membership, ct);
            await db.SaveChangesAsync(ct);
        }

        // Protocols (force plate focused)
        if (!await db.Protocols.AnyAsync(ct))
        {
            var protocols = new[]
            {
                CreateProtocol(DemoOrganizationId, "Countermovement Jump", "CMJ", "1.0", "Force Plate", "General", "Standard bilateral CMJ protocol"),
                CreateProtocol(DemoOrganizationId, "Isometric Mid-Thigh Pull", "IMTP", "1.0", "Force Plate", "Strength", "Maximal isometric strength test"),
                CreateProtocol(DemoOrganizationId, "Drop Jump", "DJ", "1.0", "Force Plate", "Jumping", "Drop jump from 40cm box"),
            };
            await db.Protocols.AddRangeAsync(protocols, ct);
            await db.SaveChangesAsync(ct);
        }

        // Subjects / Athletes
        if (!await db.Subjects.AnyAsync(ct))
        {
            var subjects = new[]
            {
                new Subject(DemoOrganizationId, "伟", "张", SubjectType.Athlete, displayName: "Zhang Wei", sex: "M", dateOfBirth: new DateTimeOffset(2001, 3, 15, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "明", "李", SubjectType.Athlete, displayName: "Li Ming", sex: "M", dateOfBirth: new DateTimeOffset(2002, 7, 22, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "浩", "王", SubjectType.Athlete, displayName: "Wang Hao", sex: "M", dateOfBirth: new DateTimeOffset(2000, 11, 8, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "陈", "杰", SubjectType.Athlete, displayName: "Chen Jie", sex: "M", dateOfBirth: new DateTimeOffset(2003, 1, 30, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "刘洋", "", SubjectType.Athlete, displayName: "Liu Yang", sex: "M", dateOfBirth: new DateTimeOffset(1999, 5, 14, 0, 0, 0, TimeSpan.Zero)),
            };
            await db.Subjects.AddRangeAsync(subjects, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static Protocol CreateProtocol(Guid orgId, string name, string code, string version, string category, string sport, string description)
    {
        var p = new Protocol(orgId, name, code, version, category, sport, description);
        p.Publish();
        return p;
    }
}
