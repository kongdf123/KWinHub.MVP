using System.Reflection;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.ArchitectureTests;

public class ArchitectureBoundaryTests
{
    private static readonly Assembly PlatformDomain = typeof(KWinHub.Platform.Organizations.Organization).Assembly;
    private static readonly Assembly HubDomain = typeof(KWinHub.Hub.Subjects.Subject).Assembly;
    private static readonly Assembly PlatformInfrastructure = typeof(PlatformDbContext).Assembly;
    private static readonly Assembly HubInfrastructure = typeof(HubDbContext).Assembly;
    private static readonly Assembly BuildingBlocks = typeof(KWinHub.BuildingBlocks.Abstractions.ICurrentUserContext).Assembly;

    [Fact]
    public void PlatformDomain_DoesNotReference_HubDomain()
    {
        AssertNoReference(PlatformDomain, HubDomain);
    }

    [Fact]
    public void HubDomain_DoesNotReference_PlatformDomain()
    {
        AssertNoReference(HubDomain, PlatformDomain);
    }

    [Fact]
    public void PlatformInfrastructure_DoesNotReference_HubDomain()
    {
        AssertNoReference(PlatformInfrastructure, HubDomain);
    }

    [Fact]
    public void HubInfrastructure_DoesNotReference_PlatformDomain()
    {
        AssertNoReference(HubInfrastructure, PlatformDomain);
    }

    [Fact]
    public void PlatformInfrastructure_DoesNotReference_HubInfrastructure()
    {
        AssertNoReference(PlatformInfrastructure, HubInfrastructure);
    }

    [Fact]
    public void HubInfrastructure_DoesNotReference_PlatformInfrastructure()
    {
        AssertNoReference(HubInfrastructure, PlatformInfrastructure);
    }

    [Fact]
    public void BuildingBlocks_DoesNotReference_EfCore()
    {
        var referenced = BuildingBlocks.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain(referenced, name =>
            name != null && name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildingBlocks_DoesNotReference_AnyDomain()
    {
        var referenced = BuildingBlocks.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain(referenced, name =>
            name != null && (name.Contains("Platform.Domain") || name.Contains("Hub.Domain")));
    }

    [Fact]
    public void PlatformDbContext_AllEntitiesInPlatformSchema()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase("arch_platform_schema")
            .Options;

        using var db = new PlatformDbContext(options);

        foreach (var entityType in db.Model.GetEntityTypes())
        {
            var schema = entityType.GetSchema();
            Assert.Equal(
                PlatformDbContext.SchemaName,
                schema);
        }
    }

    [Fact]
    public void HubDbContext_AllEntitiesInHubSchema()
    {
        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseInMemoryDatabase("arch_hub_schema")
            .Options;

        using var db = new HubDbContext(options);

        foreach (var entityType in db.Model.GetEntityTypes())
        {
            var schema = entityType.GetSchema();
            Assert.Equal(
                HubDbContext.SchemaName,
                schema);
        }
    }

    [Fact]
    public void PlatformDbContext_DoesNotContain_HubOwnedEntities()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase("arch_platform_no_hub")
            .Options;

        using var db = new PlatformDbContext(options);

        var entityNames = db.Model.GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .ToHashSet();

        Assert.DoesNotContain("Subject", entityNames);
        Assert.DoesNotContain("MeasurementSession", entityNames);
        Assert.DoesNotContain("Result", entityNames);
        Assert.DoesNotContain("Protocol", entityNames);
        Assert.DoesNotContain("Metric", entityNames);
        Assert.DoesNotContain("OrganizationMembership", entityNames);
    }

    [Fact]
    public void HubDbContext_DoesNotContain_PlatformOwnedEntities()
    {
        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseInMemoryDatabase("arch_hub_no_platform")
            .Options;

        using var db = new HubDbContext(options);

        var entityNames = db.Model.GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .ToHashSet();

        Assert.DoesNotContain("Organization", entityNames);
        Assert.DoesNotContain("User", entityNames);
        Assert.DoesNotContain("SubscriptionPlan", entityNames);
        Assert.DoesNotContain("OrganizationSubscription", entityNames);
        Assert.DoesNotContain("AuditLog", entityNames);
    }

    private static void AssertNoReference(Assembly source, Assembly target)
    {
        var targetName = target.GetName().Name;
        var referenced = source.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain(referenced, name =>
            string.Equals(name, targetName, StringComparison.Ordinal));
    }
}