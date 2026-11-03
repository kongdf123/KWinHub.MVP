namespace KWinHub.Hub.Organizations;

public static class OrganizationRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Member = "Member";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Owner, Admin, Member, Viewer
    };

    public static bool IsValid(string role) => All.Contains(role);

    public static string Normalize(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return Member;
        return All.Contains(role) ? role : Member;
    }
}