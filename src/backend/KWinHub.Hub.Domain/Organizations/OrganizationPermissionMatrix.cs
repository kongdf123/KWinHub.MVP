namespace KWinHub.Hub.Organizations;

public static class OrganizationPermissions
{
    public const string Read = "read";
    public const string Write = "write";
    public const string Export = "export";
    public const string RawAsset = "raw-asset";
    public const string Report = "report";
    public const string Playback = "playback";
}

public static class OrganizationPermissionMatrix
{
    private static readonly Dictionary<string, HashSet<string>> _permissions = new()
    {
        [OrganizationRoles.Owner] = new()
        {
            OrganizationPermissions.Read,
            OrganizationPermissions.Write,
            OrganizationPermissions.Export,
            OrganizationPermissions.RawAsset,
            OrganizationPermissions.Report,
            OrganizationPermissions.Playback
        },
        [OrganizationRoles.Admin] = new()
        {
            OrganizationPermissions.Read,
            OrganizationPermissions.Write,
            OrganizationPermissions.Export,
            OrganizationPermissions.RawAsset,
            OrganizationPermissions.Report,
            OrganizationPermissions.Playback
        },
        [OrganizationRoles.Member] = new()
        {
            OrganizationPermissions.Read,
            OrganizationPermissions.Write,
            OrganizationPermissions.Export,
            OrganizationPermissions.Playback
        },
        [OrganizationRoles.Viewer] = new()
        {
            OrganizationPermissions.Read
        }
    };

    public static IReadOnlySet<string> GetPermissions(string role)
    {
        return _permissions.TryGetValue(role, out var perms)
            ? perms
            : _permissions[OrganizationRoles.Member];
    }

    public static bool HasPermission(string role, string permission)
    {
        return GetPermissions(role).Contains(permission);
    }
}