using KWinHub.Hub.Application.CrossContext;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.InternalApi;

[ApiController]
[InternalApiKey]
[Route("internal/v1/workspace-stats")]
public class InternalWorkspaceStatsController : ControllerBase
{
    private readonly IWorkspaceStatsReader _stats;

    public InternalWorkspaceStatsController(IWorkspaceStatsReader stats)
    {
        _stats = stats;
    }

    [HttpPost("organizations")]
    public async Task<IActionResult> Organizations(
        [FromBody] IdBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _stats.GetOrganizationStatsAsync(request.Ids ?? [], cancellationToken);
        return Ok(result.Select(s => new
        {
            organizationId = s.OrganizationId,
            memberCount = s.MemberCount,
            subjectCount = s.SubjectCount,
            sessionCount = s.SessionCount,
            resultCount = s.ResultCount
        }));
    }

    [HttpPost("users")]
    public async Task<IActionResult> Users(
        [FromBody] IdBatchRequest request,
        CancellationToken cancellationToken)
    {
        var counts = await _stats.CountMembershipsByUserAsync(request.Ids ?? [], cancellationToken);
        return Ok(counts.Select(kv => new
        {
            userId = kv.Key,
            membershipCount = kv.Value
        }));
    }
}

public sealed record IdBatchRequest(IReadOnlyCollection<Guid> Ids);
