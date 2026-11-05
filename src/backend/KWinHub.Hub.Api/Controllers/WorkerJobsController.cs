using KWinHub.BuildingBlocks.Abstractions;
using KWinHub.Hub.Api.Authorization;
using KWinHub.Hub.Application.Workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KWinHub.Hub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/worker-jobs")]
public class WorkerJobsController : ControllerBase
{
    private readonly IWorkerJobRepository _workerJobRepository;
    private readonly ICurrentUserContext _currentUser;

    public WorkerJobsController(
        IWorkerJobRepository workerJobRepository,
        ICurrentUserContext currentUser)
    {
        _workerJobRepository = workerJobRepository;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(IReadOnlyList<WorkerJobResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkerJobResponse>>> List(
        [FromQuery] string? status = null,
        [FromQuery] string? queue = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0 || limit > 500)
        {
            limit = 100;
        }

        var jobs = await _workerJobRepository.GetByOrganizationIdAsync(
            _currentUser.OrganizationId,
            status,
            queue,
            limit,
            cancellationToken);

        return Ok(jobs.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = HubPolicyNames.CanRead)]
    [ProducesResponseType(typeof(WorkerJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerJobResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await _workerJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null || job.OrganizationId != _currentUser.OrganizationId)
        {
            return NotFound();
        }

        return Ok(ToResponse(job));
    }

    [HttpPost("{id:guid}/replay")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Replay(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await _workerJobRepository.GetByIdAsync(id, cancellationToken);
        if (job is null || job.OrganizationId != _currentUser.OrganizationId)
        {
            return NotFound();
        }

        if (job.Status != Workers.WorkerJobStatus.Failed &&
            job.Status != Workers.WorkerJobStatus.Skipped)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Job not replayable",
                Detail = $"Only Failed or Skipped jobs can be replayed. Current status: {job.Status}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        await _workerJobRepository.ResetJobForRetryAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("replay-failed")]
    [Authorize(Policy = HubPolicyNames.CanWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> ReplayAllFailed(
        CancellationToken cancellationToken)
    {
        var count = await _workerJobRepository.ResetFailedForRetryAsync(
            _currentUser.OrganizationId,
            cancellationToken);

        return Ok(count);
    }

    private static WorkerJobResponse ToResponse(Workers.WorkerJob job) => new(
        job.Id,
        job.AssetId,
        job.OrganizationId,
        job.QueueName,
        job.JobType,
        job.Status,
        job.Attempt,
        job.MaxAttempts,
        job.PayloadJson,
        job.ResultJson,
        job.LeaseExpiresAt,
        job.LastError,
        job.CompletedAt,
        job.CreatedAt,
        job.UpdatedAt);
}