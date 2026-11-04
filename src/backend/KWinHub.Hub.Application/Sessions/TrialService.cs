using KWinHub.Hub.Contracts.Sessions;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public class TrialService : ITrialService
{
    private readonly ITrialRepository _repository;
    private readonly ISessionRepository _sessionRepository;

    public TrialService(ITrialRepository repository, ISessionRepository sessionRepository)
    {
        _repository = repository;
        _sessionRepository = sessionRepository;
    }

    public async Task<TrialResponse> CreateAsync(
        Guid organizationId,
        CreateTrialRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(organizationId, request.SessionId, cancellationToken);
        if (session is null)
        {
            throw new ArgumentException($"Session {request.SessionId} not found.");
        }

        var trial = new Trial(
            organizationId: organizationId,
            sessionId: request.SessionId,
            trialIndex: request.TrialIndex,
            externalId: request.ExternalId,
            notes: request.Notes);

        await _repository.AddAsync(trial, cancellationToken);
        return ToResponse(trial);
    }

    public async Task<TrialResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var trial = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return trial is null ? null : ToResponse(trial);
    }

    public async Task<IReadOnlyList<TrialResponse>> ListBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var trials = await _repository.ListBySessionAsync(organizationId, sessionId, cancellationToken);
        return trials.Select(ToResponse).ToList();
    }

    public async Task<TrialResponse?> CompleteAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var trial = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (trial is null) return null;

        trial.Complete();
        await _repository.UpdateAsync(trial, cancellationToken);
        return ToResponse(trial);
    }

    public async Task<TrialResponse?> FailAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var trial = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (trial is null) return null;

        trial.Fail();
        await _repository.UpdateAsync(trial, cancellationToken);
        return ToResponse(trial);
    }

    private static TrialResponse ToResponse(Trial trial) =>
        new(
            Id: trial.Id,
            OrganizationId: trial.OrganizationId,
            SessionId: trial.SessionId,
            TrialIndex: trial.TrialIndex,
            Status: trial.Status.ToString(),
            ExternalId: trial.ExternalId,
            StartedAt: trial.StartedAt,
            CompletedAt: trial.CompletedAt,
            Notes: trial.Notes,
            CreatedAt: trial.CreatedAt,
            UpdatedAt: trial.UpdatedAt);
}