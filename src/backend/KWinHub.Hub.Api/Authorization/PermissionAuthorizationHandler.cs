using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace KWinHub.Hub.Api.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IOrgContextService _orgContext;
    private readonly ICurrentUserContext _currentUser;

    public PermissionAuthorizationHandler(
        IOrgContextService orgContext,
        ICurrentUserContext currentUser)
    {
        _orgContext = orgContext;
        _currentUser = currentUser;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = _currentUser.UserId;
        var orgId = _currentUser.OrganizationId;

        if (userId == Guid.Empty || orgId == Guid.Empty)
        {
            context.Fail();
            return;
        }

        var hasPermission = await _orgContext.HasPermissionAsync(
            userId,
            orgId,
            requirement.Permission,
            CancellationToken.None);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}