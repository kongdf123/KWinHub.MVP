using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class OrganizationMembersIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public OrganizationMembersIntegrationTests(HubApiFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAsync(Action<HubDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    private HttpClient CreateAuthorizedClient(Guid userId, Guid orgId, string role = OrganizationRoles.Admin)
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId,
            role: role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static void SeedActiveMembership(HubDbContext db, Guid userId, Guid orgId, string role)
    {
        var membership = new OrganizationMembership(userId, orgId, role);
        membership.AcceptInvite();
        db.OrganizationMemberships.Add(membership);
        db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Active", 1));
    }

    private static string MembersUrl(Guid orgId) => $"/api/v1/organizations/{orgId}/members";

    [Fact]
    public async Task ListMembers_ReturnsMembers()
    {
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            SeedActiveMembership(db, memberId, orgId, OrganizationRoles.Member);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.GetAsync(MembersUrl(orgId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<MemberResponseDto>>();
        Assert.NotNull(members);
        Assert.Equal(2, members.Count);
        Assert.Contains(members, m => m.UserId == memberId);
    }

    [Fact]
    public async Task ChangeRole_Admin_CanChangeMemberRole()
    {
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            SeedActiveMembership(db, memberId, orgId, OrganizationRoles.Member);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PutAsJsonAsync(
            $"{MembersUrl(orgId)}/{memberId}/role",
            new { role = OrganizationRoles.Admin });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberResponseDto>();
        Assert.NotNull(member);
        Assert.Equal(OrganizationRoles.Admin, member.Role);
    }

    [Fact]
    public async Task ChangeRole_InvalidRole_ReturnsBadRequest()
    {
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            SeedActiveMembership(db, memberId, orgId, OrganizationRoles.Member);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PutAsJsonAsync(
            $"{MembersUrl(orgId)}/{memberId}/role",
            new { role = "InvalidRole" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Suspend_Admin_CanSuspendMember()
    {
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            SeedActiveMembership(db, memberId, orgId, OrganizationRoles.Member);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PostAsync($"{MembersUrl(orgId)}/{memberId}/suspend", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberResponseDto>();
        Assert.NotNull(member);
        Assert.Equal(OrganizationMembership.StatusSuspended, member.Status);
    }

    [Fact]
    public async Task Reactivate_Admin_CanReactivateSuspendedMember()
    {
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            var membership = new OrganizationMembership(memberId, orgId, OrganizationRoles.Member);
            membership.AcceptInvite();
            membership.Suspend();
            db.OrganizationMemberships.Add(membership);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PostAsync($"{MembersUrl(orgId)}/{memberId}/reactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<MemberResponseDto>();
        Assert.NotNull(member);
        Assert.Equal(OrganizationMembership.StatusActive, member.Status);
    }

    [Fact]
    public async Task Invite_Admin_CreatesInvitation()
    {
        var adminId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PostAsJsonAsync(
            $"{MembersUrl(orgId)}/invitations",
            new { email = "invitee@example.com", role = OrganizationRoles.Member });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var invitation = await response.Content.ReadFromJsonAsync<InvitationResponseDto>();
        Assert.NotNull(invitation);
        Assert.Equal("invitee@example.com", invitation.Email);
        Assert.Equal(OrganizationRoles.Member, invitation.Role);
        Assert.Equal(OrganizationInvitation.StatusPending, invitation.Status);
        Assert.True(invitation.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ListInvitations_ReturnsInvitations()
    {
        var adminId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            db.OrganizationInvitations.Add(new OrganizationInvitation(
                orgId, "user1@example.com", OrganizationRoles.Member, adminId, TimeSpan.FromDays(7)));
            db.OrganizationInvitations.Add(new OrganizationInvitation(
                orgId, "user2@example.com", OrganizationRoles.Admin, adminId, TimeSpan.FromDays(7)));
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.GetAsync($"{MembersUrl(orgId)}/invitations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invitations = await response.Content.ReadFromJsonAsync<List<InvitationResponseDto>>();
        Assert.NotNull(invitations);
        Assert.Equal(2, invitations.Count);
    }

    [Fact]
    public async Task ListInvitations_WithStatusFilter_ReturnsFiltered()
    {
        var adminId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            var pending = new OrganizationInvitation(
                orgId, "pending@example.com", OrganizationRoles.Member, adminId, TimeSpan.FromDays(7));
            var revoked = new OrganizationInvitation(
                orgId, "revoked@example.com", OrganizationRoles.Member, adminId, TimeSpan.FromDays(7));
            revoked.Revoke(adminId);
            db.OrganizationInvitations.Add(pending);
            db.OrganizationInvitations.Add(revoked);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.GetAsync($"{MembersUrl(orgId)}/invitations?status=Pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invitations = await response.Content.ReadFromJsonAsync<List<InvitationResponseDto>>();
        Assert.NotNull(invitations);
        Assert.Single(invitations);
        Assert.Equal("pending@example.com", invitations[0].Email);
    }

    [Fact]
    public async Task AcceptInvitation_ValidToken_CreatesMembership()
    {
        var adminId = Guid.NewGuid();
        var inviteeId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        string token = string.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            var invitation = new OrganizationInvitation(
                orgId, "invitee@example.com", OrganizationRoles.Member, adminId, TimeSpan.FromDays(7));
            token = invitation.Token;
            db.OrganizationInvitations.Add(invitation);
        });

        var client = CreateAuthorizedClient(inviteeId, orgId);

        var response = await client.PostAsJsonAsync(
            $"{MembersUrl(orgId)}/invitations/accept",
            new { token });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invitation = await response.Content.ReadFromJsonAsync<InvitationResponseDto>();
        Assert.NotNull(invitation);
        Assert.Equal(OrganizationInvitation.StatusAccepted, invitation.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var membership = await db.OrganizationMemberships
            .FirstOrDefaultAsync(m => m.UserId == inviteeId && m.OrganizationId == orgId);
        Assert.NotNull(membership);
        Assert.Equal(OrganizationMembership.StatusActive, membership.Status);
    }

    [Fact]
    public async Task AcceptInvitation_InvalidToken_ReturnsNotFound()
    {
        var inviteeId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var client = CreateAuthorizedClient(inviteeId, orgId);

        var response = await client.PostAsJsonAsync(
            $"{MembersUrl(orgId)}/invitations/accept",
            new { token = "nonexistent-token" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_Admin_CanRevokePendingInvitation()
    {
        var adminId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, adminId, orgId, OrganizationRoles.Admin);
            var invitation = new OrganizationInvitation(
                orgId, "invitee@example.com", OrganizationRoles.Member, adminId, TimeSpan.FromDays(7));
            invitationId = invitation.Id;
            db.OrganizationInvitations.Add(invitation);
        });

        var client = CreateAuthorizedClient(adminId, orgId);

        var response = await client.PostAsync(
            $"{MembersUrl(orgId)}/invitations/{invitationId}/revoke",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invitation = await response.Content.ReadFromJsonAsync<InvitationResponseDto>();
        Assert.NotNull(invitation);
        Assert.Equal(OrganizationInvitation.StatusRevoked, invitation.Status);
    }

    [Fact]
    public async Task WriteOperations_ViewerRole_Forbidden()
    {
        var viewerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, viewerId, orgId, OrganizationRoles.Viewer);
            SeedActiveMembership(db, memberId, orgId, OrganizationRoles.Member);
        });

        var client = CreateAuthorizedClient(viewerId, orgId, OrganizationRoles.Viewer);

        var inviteResponse = await client.PostAsJsonAsync(
            $"{MembersUrl(orgId)}/invitations",
            new { email = "test@example.com", role = OrganizationRoles.Member });
        Assert.Equal(HttpStatusCode.Forbidden, inviteResponse.StatusCode);

        var roleResponse = await client.PutAsJsonAsync(
            $"{MembersUrl(orgId)}/{memberId}/role",
            new { role = OrganizationRoles.Admin });
        Assert.Equal(HttpStatusCode.Forbidden, roleResponse.StatusCode);

        var suspendResponse = await client.PostAsync($"{MembersUrl(orgId)}/{memberId}/suspend", null);
        Assert.Equal(HttpStatusCode.Forbidden, suspendResponse.StatusCode);
    }

    private sealed record MemberResponseDto(
        Guid UserId,
        string Role,
        string Status,
        DateTimeOffset JoinedAt,
        DateTimeOffset? LastActiveAt);

    private sealed record InvitationResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Email,
        string Role,
        string Status,
        DateTimeOffset ExpiresAt,
        Guid CreatedBy,
        DateTimeOffset CreatedAt,
        DateTimeOffset? AcceptedAt,
        Guid? AcceptedBy,
        DateTimeOffset? RevokedAt,
        Guid? RevokedBy);
}