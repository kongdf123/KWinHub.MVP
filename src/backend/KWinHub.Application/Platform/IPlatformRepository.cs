using KWinHub.Domain.Audit;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Users;

namespace KWinHub.Application.Platform;

public record OrganizationListRow(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    string Status,
    string? CountryCode,
    DateTimeOffset CreatedAt,
    int MemberCount,
    int SubjectCount,
    int SessionCount,
    int ResultCount);

public record OrganizationMemberRow(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    DateTimeOffset JoinedAt);

public record UserListRow(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    string? PlatformRole,
    bool EmailVerified,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    int OrganizationCount);

public interface IPlatformRepository
{
    Task<int> CountOrganizationsAsync(
        OrganizationStatus? status,
        OrganizationType? type,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationListRow>> ListOrganizationsAsync(
        string? search,
        string? status,
        string? type,
        CancellationToken cancellationToken = default);

    Task<OrganizationListRow?> GetOrganizationRowAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationMemberRow>> ListMembersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveOrganizationAsync(CancellationToken cancellationToken = default);

    Task<int> CountUsersAsync(string? status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserListRow>> ListUsersAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveUserAsync(CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog log, CancellationToken cancellationToken = default);

    Task SaveAuditAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> ListAuditLogsAsync(
        string? entityType,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountGlobalProtocolsAsync(CancellationToken cancellationToken = default);

    Task<int> CountGlobalMetricsAsync(CancellationToken cancellationToken = default);
}
