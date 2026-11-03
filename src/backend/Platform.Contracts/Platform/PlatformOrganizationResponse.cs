namespace KWinHub.Contracts.Platform;

public record PlatformOrganizationResponse(
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

public record PlatformOrganizationDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    string Status,
    string? CountryCode,
    string? Region,
    string? Timezone,
    string? Locale,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PlatformOrganizationMemberResponse> Members);

public record PlatformOrganizationMemberResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    DateTimeOffset JoinedAt);
