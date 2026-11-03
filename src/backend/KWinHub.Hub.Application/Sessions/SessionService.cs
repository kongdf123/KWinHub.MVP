using KWinHub.Application.Protocols;
using KWinHub.Application.Results;
using KWinHub.Application.Subscriptions;
using KWinHub.Contracts.Sessions;
using KWinHub.Domain.Results;
using KWinHub.Domain.Sessions;
using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;
    private readonly IProtocolRepository _protocolRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IUsageMeter _usageMeter;

    public SessionService(
        ISessionRepository repository,
        IProtocolRepository protocolRepository,
        IResultRepository resultRepository,
        IUsageMeter usageMeter)
    {
        _repository = repository;
        _protocolRepository = protocolRepository;
        _resultRepository = resultRepository;
        _usageMeter = usageMeter;
    }

    public async Task<SessionResponse> CreateAsync(
        Guid organizationId,
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = new MeasurementSession(
            organizationId: organizationId,
            subjectId: request.SubjectId,
            protocolId: request.ProtocolId,
            protocolVersion: request.ProtocolVersion,
            sourceId: request.SourceId,
            deviceId: request.DeviceId,
            clientApplication: request.ClientApplication,
            clientVersion: request.ClientVersion,
            externalId: request.ExternalId,
            timezone: request.Timezone);

        // Enforce monthly sessions quota before persisting. Uses the stable
        // business event id (external id, falling back to the session id) so
        // retries are idempotent.
        var businessEventId = string.IsNullOrWhiteSpace(request.ExternalId)
            ? session.Id.ToString()
            : request.ExternalId;
        await _usageMeter.ConsumeAsync(
            organizationId,
            SubscriptionCatalog.QuotaSessionsMonthly,
            businessEventId,
            delta: 1,
            cancellationToken);

        await _repository.AddAsync(session, cancellationToken);

        // Simulated analysis (inline — fine for MVP)
        var protocol = await _protocolRepository.GetByIdAsync(
            organizationId, session.ProtocolId, cancellationToken);

        if (protocol is not null)
        {
            var result = ResultSimulator.CreateSimulatedResult(session, protocol);
            await _resultRepository.AddAsync(result, cancellationToken);

            session.TransitionTo(SessionStatus.Published);
            await _repository.UpdateAsync(session, cancellationToken);
        }

        return ToResponse(session);
    }

    public async Task<SessionResponse> GetOrCreateByExternalIdAsync(
        Guid organizationId,
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.ExternalId))
        {
            var existing = await _repository.GetByExternalIdAsync(
                organizationId,
                request.ExternalId,
                cancellationToken);

            if (existing is not null)
            {
                return ToResponse(existing);
            }
        }

        return await CreateAsync(organizationId, request, cancellationToken);
    }

    public async Task<IReadOnlyList<SessionResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return sessions.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<SessionResponse>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.ListBySubjectAsync(organizationId, subjectId, cancellationToken);
        return sessions.Select(ToResponse).ToList();
    }

    public async Task<SessionResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return session is null ? null : ToResponse(session);
    }

    public async Task<SessionStatusResponse?> GetStatusAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var session = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return new SessionStatusResponse(
            SessionId: session.Id,
            Status: session.Status.ToString(),
            UpdatedAt: session.UpdatedAt);
    }

    private static SessionResponse ToResponse(MeasurementSession session) =>
        new(
            Id: session.Id,
            OrganizationId: session.OrganizationId,
            SubjectId: session.SubjectId,
            ProtocolId: session.ProtocolId,
            ProtocolVersion: session.ProtocolVersion,
            SourceId: session.SourceId,
            DeviceId: session.DeviceId,
            Status: session.Status.ToString(),
            StartedAt: session.StartedAt,
            CompletedAt: session.CompletedAt,
            ClientApplication: session.ClientApplication,
            ClientVersion: session.ClientVersion,
            ExternalId: session.ExternalId,
            Timezone: session.Timezone,
            CreatedAt: session.CreatedAt,
            UpdatedAt: session.UpdatedAt);
}
