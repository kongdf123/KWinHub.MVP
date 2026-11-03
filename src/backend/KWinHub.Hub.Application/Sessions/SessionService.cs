using KWinHub.Hub.Application.CrossContext;
using KWinHub.Hub.Contracts.Sessions;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;
    private readonly IPlatformGateway _platformGateway;

    public SessionService(
        ISessionRepository repository,
        IPlatformGateway platformGateway)
    {
        _repository = repository;
        _platformGateway = platformGateway;
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

        // Enforce monthly sessions quota before persisting. The Platform context
        // owns all plan/quota logic; the Hub only reports the business event. The
        // stable business event id makes retries idempotent.
        var businessEventId = string.IsNullOrWhiteSpace(request.ExternalId)
            ? session.Id.ToString()
            : request.ExternalId;
        await _platformGateway.ConsumeUsageAsync(
            organizationId,
            HubUsageMetrics.SessionsMonthly,
            businessEventId,
            delta: 1,
            cancellationToken);

        await _repository.AddAsync(session, cancellationToken);

        // Sessions are persisted in their initial (Received) state. Scientific
        // results are produced only by the real analysis pipeline; the Hub never
        // fabricates metrics (ADR-0001 — ResultSimulator removed from production).
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
